using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalUserDataSaveOnPop : MonoBehaviour, M8.IModalPop {
    public M8.UserData userData;

    void M8.IModalPop.Pop() {
        userData.Save();
    }
}
