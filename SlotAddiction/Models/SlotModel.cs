using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SlotAddiction.Models
{
    [Table("SlotModel")]
    [Serializable]
    public class SlotModel
    {
        public int Id { get; set; }
        public string SlotModelName { get; set; }
        public string SlotModelShortName { get; set; }
        public string ThroughType { get; set; }
    }
}
//public static string Basilisk_Kizuna = "ﾊﾞｼﾞﾘｽｸ 甲賀忍法帖 絆";
//public static List<string> ThroughCountAimList = new List<string>
//        {
//            Basilisk_Kizuna,
//        };
//    }