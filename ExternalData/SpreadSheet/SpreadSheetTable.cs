using System.Collections.Generic;
using System.IO;
using CsvFiles;
using UnityEngine.Assertions;

namespace GameLib.ExternalData
{

    public class SpreadSheetTable<T> where T : new()
    {
        private RoTable<T> _table = new RoTable<T>();

        public SpreadSheetTable(StringReader sr)
        {
            Assert.IsNotNull(sr);
            LoadDataTable(sr);
        }

        public SpreadSheetTable(List<T> rows)
        {
            foreach (var row in rows)
                _table.Data.Add(row);
        }

        public RoTable<T> Table()
        {
            return _table;
        }

        private void LoadDataTable(StringReader sr)
        {
            foreach (var row in CsvFile.Read<T>(sr))
                _table.Data.Add(row);
        }
    }
}
