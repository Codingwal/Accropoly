using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWorkplace : IMapTile
{
    public List<IPerson> Workers { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="person"></param>
    /// <returns>Returns whether the person was employed or not.</returns>
    public bool AddWorker(IPerson person);
}
