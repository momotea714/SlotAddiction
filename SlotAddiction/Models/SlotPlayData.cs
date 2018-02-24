using System.Collections.Generic;
using SlotAddiction.Const;

namespace SlotAddiction.Models
{
    /// <summary>
    /// 遊技台の情報
    /// </summary>
    public class SlotPlayData
    {
        /// <summary>
        /// 設置店名
        /// </summary>
        public string StoreName { get; set; }
        /// <summary>
        /// 台名
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// コイン単価
        /// </summary>
        public decimal CoinPrice { get; set; }
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
        /// <summary>
        /// ステータス
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 当選履歴
        /// </summary>
        public List<WiningType> WiningHistory { get; set; }
    }
}
