using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GameMeta
{
    /// <summary>槽位读档结果</summary>
    public class SlotLoadResult
    {
        public bool success;
        public RunSaveData data;
        public string error;
        public static SlotLoadResult Ok(RunSaveData d) => new SlotLoadResult { success = true, data = d };
        public static SlotLoadResult Fail(string e) => new SlotLoadResult { success = false, error = e };
    }

    /// <summary>
    /// 多槽位存档：PlayerPrefs + JSON，XOR 加密 + Base64 编码 + FNV 校验和防篡改。
    /// 仅手动保存时写入，游戏运行期间无任何自动写入单局存档的路径。
    /// </summary>
    public static class SaveManager
    {
        const string MetaKey = "MetaSave_v1";
        const string SlotKeyPrefix = "SaveSlot_v1_";
        const string SfxVolumeKey = "SfxVolume_v1";
        const string BgmVolumeKey = "BgmVolume_v1";

        public const int MaxSlots = 10;

        // 轻量本地密钥（防普通玩家直接改 PlayerPrefs；非密码学级安全）
        static readonly byte[] Secret = Encoding.UTF8.GetBytes("NeonSurvivor_SaveKey_2026");

        // ===== 永久元数据 =====
        public static bool HasMeta() => PlayerPrefs.HasKey(MetaKey);

        public static MetaSaveData LoadMeta()
        {
            if (!PlayerPrefs.HasKey(MetaKey)) return new MetaSaveData();
            try
            {
                return JsonUtility.FromJson<MetaSaveData>(PlayerPrefs.GetString(MetaKey))
                       ?? new MetaSaveData();
            }
            catch
            {
                Debug.LogWarning("永久存档解析失败，使用新存档");
                return new MetaSaveData();
            }
        }

        public static void SaveMeta(MetaSaveData data)
        {
            PlayerPrefs.SetString(MetaKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        // ===== 槽位查询 =====
        public static string SlotName(int index) => "存档" + (index + 1);
        static string SlotKey(int index) => SlotKeyPrefix + index;

        public static bool HasSlot(int index) =>
            index >= 0 && index < MaxSlots && PlayerPrefs.HasKey(SlotKey(index));

        public static bool HasAnySlot()
        {
            for (int i = 0; i < MaxSlots; i++)
                if (PlayerPrefs.HasKey(SlotKey(i))) return true;
            return false;
        }

        /// <summary>槽位索引列表，按存档时间倒序（最新在前）；损坏但存在的槽位也列出</summary>
        public static List<int> ListSlotIndices()
        {
            var result = new List<int>();
            for (int i = 0; i < MaxSlots; i++)
                if (PlayerPrefs.HasKey(SlotKey(i))) result.Add(i);

            result.Sort((a, b) =>
            {
                var da = LoadSlotRaw(a);
                var db = LoadSlotRaw(b);
                long ta = da != null ? da.saveUnixTime : 0;
                long tb = db != null ? db.saveUnixTime : 0;
                return tb.CompareTo(ta);
            });
            return result;
        }

        /// <summary>最新可正常读取的槽位索引；无存档或全部损坏时返回 -1</summary>
        public static int LatestSlotIndex()
        {
            foreach (int index in ListSlotIndices())
            {
                if (LoadSlotRaw(index) != null) return index;
            }
            return -1;
        }

        /// <summary>第一个空槽位；槽位满返回 -1</summary>
        public static int FirstEmptySlot()
        {
            for (int i = 0; i < MaxSlots; i++)
                if (!PlayerPrefs.HasKey(SlotKey(i))) return i;
            return -1;
        }

        // ===== 写入 / 读取（含加密与校验）=====
        public static bool SaveToSlot(int index, RunSaveData data)
        {
            if (index < 0 || index >= MaxSlots) return false;
            try
            {
                data.slotIndex = index;
                data.slotName = SlotName(index);
                data.saveUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                string json = JsonUtility.ToJson(data);
                data.payloadBytes = Encoding.UTF8.GetByteCount(json);

                json = JsonUtility.ToJson(data);   // 回填大小后再序列化一次
                PlayerPrefs.SetString(SlotKey(index), Encode(json));
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("存档失败：" + e.Message);
                return false;
            }
        }

        /// <summary>读档（解密 + 校验和验证）；损坏/被篡改返回失败结果</summary>
        public static SlotLoadResult LoadSlot(int index)
        {
            if (!HasSlot(index)) return SlotLoadResult.Fail("存档不存在");
            try
            {
                string json = Decode(PlayerPrefs.GetString(SlotKey(index)));
                var data = JsonUtility.FromJson<RunSaveData>(json);
                if (data == null) return SlotLoadResult.Fail("存档内容为空");
                return SlotLoadResult.Ok(data);
            }
            catch (Exception e)
            {
                return SlotLoadResult.Fail("存档已损坏或被修改：" + e.Message);
            }
        }

        /// <summary>仅供列表排序使用：失败返回 null，不弹错</summary>
        static RunSaveData LoadSlotRaw(int index)
        {
            try
            {
                if (!PlayerPrefs.HasKey(SlotKey(index))) return null;
                return JsonUtility.FromJson<RunSaveData>(Decode(PlayerPrefs.GetString(SlotKey(index))));
            }
            catch { return null; }
        }

        public static void DeleteSlot(int index)
        {
            if (HasSlot(index))
            {
                PlayerPrefs.DeleteKey(SlotKey(index));
                PlayerPrefs.Save();
            }
        }

        // ===== 加密：XOR → Base64，前缀 FNV-1a 校验和 =====
        static string Encode(string json)
        {
            byte[] raw = Encoding.UTF8.GetBytes(json);
            byte[] xored = Xor(raw);
            return Fnv1a(raw).ToString("X8") + ":" + Convert.ToBase64String(xored);
        }

        static string Decode(string stored)
        {
            int sep = stored.IndexOf(':');
            if (sep <= 0) throw new FormatException("缺少校验头");

            string checksum = stored.Substring(0, sep);
            byte[] xored = Convert.FromBase64String(stored.Substring(sep + 1));
            byte[] raw = Xor(xored);

            string actual = Fnv1a(raw).ToString("X8");
            if (!string.Equals(actual, checksum, StringComparison.OrdinalIgnoreCase))
                throw new FormatException("校验和不匹配");

            return Encoding.UTF8.GetString(raw);
        }

        static byte[] Xor(byte[] data)
        {
            var outBytes = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                outBytes[i] = (byte)(data[i] ^ Secret[i % Secret.Length]);
            return outBytes;
        }

        static uint Fnv1a(byte[] data)
        {
            const uint offset = 2166136261;
            const uint prime = 16777619;
            uint hash = offset;
            foreach (byte b in data)
            {
                hash ^= b;
                hash *= prime;
            }
            return hash;
        }

        // ===== 音量（UI 先行，暂不接实际音源）=====
        public static float GetSfxVolume() => PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        public static float GetBgmVolume() => PlayerPrefs.GetFloat(BgmVolumeKey, 1f);
        public static void SetSfxVolume(float v) => PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(v));
        public static void SetBgmVolume(float v) => PlayerPrefs.SetFloat(BgmVolumeKey, Mathf.Clamp01(v));
    }
}
