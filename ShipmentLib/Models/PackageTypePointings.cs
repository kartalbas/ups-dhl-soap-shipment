using System.Collections.Generic;

namespace ShipmentLib
{
    public class PackageTypePointings
    {
        private List<PackageTypePointings> _cobjList;

        public int ForeignId { get; set; }
        public string Naming { get; set; }
        public string InternalId { get; set; }

        public PackageTypePointings()
        {
            _cobjList = new List<PackageTypePointings>();
        }

        public void Add(PackageTypePointings objPtP)
        {
            _cobjList.Add(objPtP);
        }

        public PackageTypePointings Find(int iForeignId)
        {
            if (_cobjList == null)
                return null;

            for (int i = 0; i < _cobjList.Count; i++ )
            {
                if (_cobjList[i].ForeignId == iForeignId)
                    return _cobjList[i];
            }

            return null;
        }
    }
}
