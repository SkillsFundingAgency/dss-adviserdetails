using System;

namespace NCS.DSS.AdviserDetail.Cosmos.Helper
{
    public interface IDocumentDBHelper
    {
        Uri CreateDocumentCollectionUri();
        Uri CreateDocumentUri(Guid adviserDetailIdId);
        Uri CreateCustomerDocumentCollectionUri();
    }
}