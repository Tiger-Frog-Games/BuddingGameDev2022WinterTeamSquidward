using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class SellSheepUIHelper : MonoBehaviour
    {
        #region Variables

        [SerializeField] private TMP_Text nameText;

        private int taskNumber;

        private Animal animal;

        #endregion

        #region Unity Methods

        #endregion

        #region Methods
        public void setUp(Animal animalIn, int taskNum)
        {
            nameText.text = animalIn.getSheepName();
            name = "Sheep Sell Block - " + animalIn.getSheepName();

            animal = animalIn;
            taskNumber = taskNum;
        }

        public void SellAnimal()
        {
            SheepPen.Instance.sellSheep(animal);
            SheepTaskManager.Instance.ClearTask(taskNumber);
        }
        #endregion
    }
}