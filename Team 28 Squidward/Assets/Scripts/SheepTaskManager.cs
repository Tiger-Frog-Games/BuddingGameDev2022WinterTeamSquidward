using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TeamSquidward.Eric
{
    public class SheepTaskManager : MonoBehaviour
    {
        private static SheepTaskManager _instance;
        public static SheepTaskManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SheepTaskManager();
                }
                return _instance;
            }
        }

        #region Variables

        [SerializeField] private int livesLeft;

        [SerializeField] private EventChannelSO OnDayOver;
        [SerializeField] private Animator CanvasAnimator;

        [SerializeField] private Transform sellSheepUIHolder;
        [SerializeField] private GameObject sellSheepUIPrefabUI;

        [SerializeField] private TaskUIHelper helperOne;
        [SerializeField] private TaskUIHelper helperTwo;
        [SerializeField] private TaskUIHelper helperThree;

        [SerializeField] private Button buttonOne;
        [SerializeField] private Button buttonTwo;
        [SerializeField] private Button buttonThree;

        private List<Animal> validSheepForTaskOne = new List<Animal>();
        private List<Animal> validSheepForTaskTwo = new List<Animal>();
        private List<Animal> validSheepForTaskThree = new List<Animal>();



        private Task[] currentTasks = new Task[3];


        private void Start()
        {
            _instance = this;

            OnDayOver.OnEvent += OnDayOver_OnEvent;

            buttonOne.onClick.AddListener(ButtonOnePressed);
            buttonTwo.onClick.AddListener(ButtonTwoPressed);
            buttonThree.onClick.AddListener(ButtonThreePressed);

            livesLeft = 3;

            for (int i = 0; i < 3; i++)
            {
                currentTasks[i] = new Task();
                populateUI(currentTasks[i], i);
            }

            OnDayOver_OnEvent();
        }

        private void OnDestroy()
        {
            OnDayOver.OnEvent -= OnDayOver_OnEvent;

            buttonOne.onClick.RemoveListener(ButtonOnePressed);
            buttonTwo.onClick.RemoveListener(ButtonTwoPressed);
            buttonThree.onClick.RemoveListener(ButtonThreePressed);
        }

        #endregion

        #region Methods

        private void OnDayOver_OnEvent()
        {
            activeTask = -1;

            checkSheepToSell();

        }

        public void checkSheepToSell()
        {

            validSheepForTaskOne.Clear();
            validSheepForTaskTwo.Clear();
            validSheepForTaskThree.Clear();

            validSheepForTaskOne = SheepPen.Instance.CheckForValidSheep(currentTasks[0]);
            validSheepForTaskTwo = SheepPen.Instance.CheckForValidSheep(currentTasks[1]);
            validSheepForTaskThree = SheepPen.Instance.CheckForValidSheep(currentTasks[2]);



            if (currentTasks[0].isSoldThisTurn == false && validSheepForTaskOne.Count > 0)
            {
                buttonOne.interactable = true;
            }
            else
            {
                buttonOne.interactable = false;
            }

            if (currentTasks[1].isSoldThisTurn == false && validSheepForTaskTwo.Count > 0)
            {
                buttonTwo.interactable = true;
            }
            else
            {
                buttonTwo.interactable = false;
            }

            if (currentTasks[2].isSoldThisTurn == false && validSheepForTaskThree.Count > 0)
            {
                buttonThree.interactable = true;
            }
            else
            {
                buttonThree.interactable = false;
            }
        }

        private void ButtonOnePressed()
        {
            TaskButtonPress(0);
        }

        private void ButtonTwoPressed()
        {
            TaskButtonPress(1);
        }

        private void ButtonThreePressed()
        {
            TaskButtonPress(2);
        }

        private int activeTask = -1;
        SellSheepUIHelper sellSheepHolder;
        private void TaskButtonPress(int buttonPressed)
        {
            activeTask = buttonPressed;

            //delete ui elements 
            foreach(Transform child in sellSheepUIHolder)
            {
                //I Dont like this but w/e
                Destroy( child.gameObject);
            }

            

            //populate active selling sheep 
            if (activeTask == 0)
            {
                foreach(Animal animal in validSheepForTaskOne)
                {
                    sellSheepHolder = Instantiate(sellSheepUIPrefabUI, sellSheepUIHolder).GetComponent<SellSheepUIHelper>();
                    sellSheepHolder.setUp(animal,0);
                }
            }
            else if (activeTask == 1)
            {
                foreach (Animal animal in validSheepForTaskTwo)
                {
                    sellSheepHolder = Instantiate(sellSheepUIPrefabUI, sellSheepUIHolder).GetComponent<SellSheepUIHelper>();
                    sellSheepHolder.setUp(animal,1);
                }
            }
            else if (activeTask == 2)
            {
                foreach (Animal animal in validSheepForTaskThree)
                {
                    sellSheepHolder = Instantiate(sellSheepUIPrefabUI, sellSheepUIHolder).GetComponent<SellSheepUIHelper>();
                    sellSheepHolder.setUp(animal,2);
                }
            }

            UIAnimator.Instance.showSheepSellScreen();
        }

        private void populateUI(Task task,int i)
        {
            if (i == 0)
            {
                helperOne.populateUI(task);
            }
            else if (i == 1)
            {
                helperTwo.populateUI(task);
            }
            else if (i == 2)
            {
                helperThree.populateUI(task);
            }
        }

        private void cleanUI(int i)
        {
            if (i == 0)
            {
                helperOne.cleanUI();
            }else if(i == 1)
            {
                helperTwo.cleanUI();
            }
            else if (i == 2)
            {
                helperThree.cleanUI();
            }
        }

        public void ClearTask(int i)
        {
            cleanUI(i);
            currentTasks[i].isSoldThisTurn = true;
            checkSheepToSell();
        }

        public void OnNextDayButton()
        {

            for (int i = 0; i < 3; i++)
            {
                if( currentTasks[i].isTaskOver() || currentTasks[i].isSoldThisTurn )
                {
                    if (! currentTasks[i].isSoldThisTurn)
                    {
                        livesLeft--;
                        if (livesLeft <= 0)
                        {
                            print("Game Over");
                        }
                    }
                  
                    //cleanUI(i);
                    currentTasks[i] = new Task();

                    if (i == 0)
                    {
                        helperOne.populateUI(currentTasks[i]);
                    }
                    else if (i == 1)
                    {
                        helperTwo.populateUI(currentTasks[i]);
                    }
                    else if (i == 2)
                    {
                        helperThree.populateUI(currentTasks[i]);
                    }

                }
                else
                {
                    if (i == 0)
                    {
                        helperOne.refreshDaysLeft();
                    }
                    else if (i == 1)
                    {
                        helperTwo.refreshDaysLeft();
                    }
                    else if (i == 2)
                    {
                        helperThree.refreshDaysLeft();
                    }
                }
            }

            CanvasAnimator.SetTrigger("OnNightTimeEventOver");
        }

        #endregion
    }
}