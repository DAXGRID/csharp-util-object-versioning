using System;
using System.Collections.Generic;
using System.Text;

namespace DAX.ObjectVersioning.Core
{
    public interface IVersionedObject
    {
        /// <summary>
        /// Called by the versioned object manager to get the unique id of the object.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// The version in which the object were created.
        /// Called by the versioned object manager on commit, if the object is added as part of the transaction.
        /// </summary>
        IVersion CreationVersion { get;  set; }

        /// <summary>
        /// The version where the object was deleted.
        /// Called by the versioned object manager on commit, if the object is deleted as part of the transaction.
        /// </summary>
        IVersion DeletionVersion { get; set; }
    }
}
