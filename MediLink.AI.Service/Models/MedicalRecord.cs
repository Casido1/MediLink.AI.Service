using Microsoft.Extensions.VectorData;

namespace MediLink.AI.Service.Models
{
    public class MedicalRecord
    {
        [VectorStoreKey]
        public string Id { get; set; }

        [VectorStoreData]
        public string Content { get; set; }

        [VectorStoreVector(768)]
        public ReadOnlyMemory<float> Embedding { get; set; }
    }
}
