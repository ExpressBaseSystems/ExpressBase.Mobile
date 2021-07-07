using ExpressBase.Mobile.Data;

namespace ExpressBase.Mobile.Services.LocalDB
{
    public interface IDBService
    {
        void ImportData(EbDataSet dataSet);

        void CreateTables(SQLiteTableSchemaList SQLSchemaList);
    }
}
