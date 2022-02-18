using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class Rockscript : MonoBehaviour
    {
    private void onCollisionEnter(Collision col)
        {
            print(col.gameObject.name);
        }
    }
}
