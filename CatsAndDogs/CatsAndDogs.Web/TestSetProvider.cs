using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CatsAndDogs.Web
{
    public class TestSetProvider
    {
        private int maxId;
        private IReadOnlyDictionary<int, string> _trainingFiles;
        public TestSetProvider(string trainingFolderPath)
        {
            var dic = new Dictionary<int, string>();
            var files = System.IO.Directory.GetFiles(trainingFolderPath);
            int num = 0;
            foreach (var file in files) {
                dic.Add(num, file);
                num++;
            }
            _trainingFiles = new ReadOnlyDictionary<int, string>(dic);
            maxId = _trainingFiles.Keys.Max();
        }

        public string FindById(int id)
        {
            if (_trainingFiles.ContainsKey(id))
                return _trainingFiles[id];
            return null;
        }
        public int MaxId => maxId;
        
    }
}
