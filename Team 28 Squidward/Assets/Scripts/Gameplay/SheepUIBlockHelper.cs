using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class SheepUIBlockHelper : MonoBehaviour
    {
        #region Variables

        [SerializeField] private TMP_Text nameText;

        private Animal animal;
        #endregion

        #region Unity Methods

        #endregion

        #region Methods

        public void setUp(Animal animalIn)
        {
            nameText.text = animalIn.getSheepName();
            name = "Sheep Block - " + animalIn.getSheepName();

            animal = animalIn;
        }

        public void SpawnAnimal()
        {
            SheepPen.Instance.spawnExistingSheep(animal);
        }
        #endregion
    }
}