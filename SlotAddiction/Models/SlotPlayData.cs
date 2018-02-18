using System;

namespace SlotAddiction.Models
{
    /// <summary>
    /// 遊技台の情報
    /// </summary>
    public class SlotPlayData
    {
        /// <summary>
        /// 台名
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// コイン単価
        /// </summary>
        public int CoinPrice { get; set; }
        /// <summary>
        /// 台番号
        /// </summary>
        public int MachineNO { get; set; }
        /// <summary>
        /// 最終更新日時
        /// </summary>
        public string LatestUpdateDatetime { get; set; }
        /// <summary>
        /// BB回数
        /// </summary>
        public int BigBonusCount { get; set; }
        /// <summary>
        /// RB回数
        /// </summary>
        public int RegulerBonusCount { get; set; }
        /// <summary>
        /// ART回数
        /// </summary>
        public int ARTCount { get; set; }
        /// <summary>
        /// 現在の回転数
        /// </summary>
        public int StartCount { get; set; }
    }
}
