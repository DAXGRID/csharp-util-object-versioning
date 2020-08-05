using System;
using System.Collections.Generic;
using System.Text;

namespace DAX.ObjectVersioning.Core.Memory
{
    public class InMemoryVersion : IVersion
    {
        private readonly long _internalVersionId;
        private readonly string _customVersionId;
        public long InternalVersionId => _internalVersionId;
        public string CustomVersionId => _customVersionId;

        public InMemoryVersion(long internalVersionId, string customVersionId)
        {
            _internalVersionId = internalVersionId;
            _customVersionId = customVersionId;
        }

        public override string ToString()
        {
            var result =  "Version: " + _internalVersionId;

            if (_customVersionId != null)
                result += " (" + _customVersionId + ")";

            return result;
        }
    }
}
