using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectionPersistence
{
    void Save(string id);
    string Load();
    void Clear();
}
