using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TeamSquidward.Eric
{
    public class GameStateHelperUI : MonoBehaviour
    {
        /// <summary>
        ///  1 - Gameplay
        ///  2 - Nighttime 
        ///  3 - Paused
        /// </summary>
        /// <param name="newGameState"></param>
        public void changeGameState(int newGameState)
        {
            if (newGameState == 0)
            {
                GameStateManager.Instance.SetState(GameState.Gameplay);
            }
            else if(newGameState == 1)
            {
                GameStateManager.Instance.SetState(GameState.Night);
            }
            else if(newGameState == 2)
            {
                GameStateManager.Instance.SetState(GameState.Paused);
            }
            else
            {
                Debug.Log("Not a valid Game State");
            }

        }
    }
}
