using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SlotAddiction.Models
{
    [Table("Tempo")]
    [Serializable]
    public class Tempo
    {
        public int Id { get; set; }
        public string StoreURL { get; set; }
        public string StoreName { get; set; }
        public int SlotMachineStartNo { get; set; }
        public int SlotMachineEndNo { get; set; }
    }
}
