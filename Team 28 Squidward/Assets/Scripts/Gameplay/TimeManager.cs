using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Rat
{
    /// <summary>
    /// The point of this class is to run the game clock logic. It will broad cast an event whenever a minitue or an hour rolls over. 
    /// 
    /// It will reset when you reach a new day (Ive set up an empty method for is the only thing for you to fill out) OnNewDayStart()
    /// It will pause when you are in a pause menu (dont worry about this Ill do this)
    /// 
    /// 
    /// </summary>

    public class TimeManager : MonoBehaviour
    {
        #region Variables

        [SerializeField] private int startHour;
        [SerializeField] private int startMin;

        [SerializeField] private int currentHour_InGame;
        [SerializeField] private int currentMin_InGame;

        [SerializeField] private int endHour;
        [SerializeField] private int endMin;

        private float startingTime;

        /// <summary>
        /// DO NOT SET THIS IN INSPECTOR this is public for now just to see the current time.
        /// This is a float the represents how much time has passed per update method.
        /// </summary>
        [SerializeField] private float currentTime_RealTime;

        /// <summary>
        /// DO NOT SET THIS IN INSEPCTOR this is public for now just to see the current time.
        /// </summary>

        [SerializeField] private float currentTime_InGame;

        /// <summary>
        /// How much time real time in seconds is equal to one in game min
        /// </summary>
        [SerializeField] private float gameRate;

        //These are events that other classes/gameObjects will use to determine the time.

        [SerializeField] private EventChannelSOInt hourChange;
        [SerializeField] private EventChannelSOInt minChange;

        [SerializeField] private EventChannelSO OnDayOverEvent;
        [SerializeField] private Animator manuAnimator;
        //Lisens to this event
        [SerializeField] private EventChannelSO OnTimerReset;

        #endregion



        #region Unity Methods
        //Ignore this method
        private void Start()
        {
            if (gameRate <= 0)
            {
                Debug.LogError("Game rate can not be zero or negative");
            }

            currentTime_RealTime = (((startHour * 3600) + (startMin * 60)) / gameRate);
        }

        /// <summary>
        /// 
        /// Time.deltaTime is real time seconds
        /// 
        /// currentTime is the raw value of time in real time seconds of the game
        /// 
        /// So you need to find a way to change 
        /// 
        /// </summary>

        
        private void Update()
        {
            //this sets the current time 
            currentTime_RealTime += Time.deltaTime;
            //Convert current real time to game time
            currentTime_InGame = currentTime_RealTime * gameRate;

            int alreadyBroadCastedMin = currentMin_InGame;
            int alreadyBroadCastedHour = currentHour_InGame;

            currentMin_InGame = (int) currentTime_InGame / 60;
            currentHour_InGame = (int)currentTime_InGame / 3600;

            if (currentMin_InGame != alreadyBroadCastedMin)
            {
                //print("Min: " + currentMin_InGame);
                minChange.RaiseEvent( currentMin_InGame );
            }

            if (currentHour_InGame != alreadyBroadCastedHour)
            {
                //print("Hour: " + currentHour_InGame);
                hourChange.RaiseEvent(currentHour_InGame);
            }

            if ( currentTime_InGame >=  (endHour * 3600 ) + ( endMin * 60 ) )
            {
                TeamSquidward.Eric.GameStateManager.Instance.SetState(TeamSquidward.Eric.GameState.Night);
                OnDayOverEvent.RaiseEvent();
                manuAnimator.SetTrigger("OnDayOver");
            }
        }

        //Ignore this method
        private void Awake()
        {
            TeamSquidward.Eric.GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
            OnTimerReset.OnEvent += ResetTimer;
        }
        //Ignore this method
        private void OnDestroy()
        {
            TeamSquidward.Eric.GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            OnTimerReset.OnEvent -= ResetTimer;
        }

        #endregion

        #region Methods
        //Ignore this method
        private void OnGameStateChanged(TeamSquidward.Eric.GameState newGameState)
        {
            enabled = newGameState == TeamSquidward.Eric.GameState.Gameplay;
        }
        
        //reset currentTime to start a new day *Hint it is only one line*
        private void ResetTimer()
        {
            currentTime_RealTime = (((startHour * 3600) + (startMin * 60)) / gameRate);
        }

        #endregion
    }
}