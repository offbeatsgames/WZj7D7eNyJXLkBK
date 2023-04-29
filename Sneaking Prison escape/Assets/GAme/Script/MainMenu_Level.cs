using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu_Level : MonoBehaviour
{
	int levelNumber = 1;
	public Text TextLevel;
	public GameObject Locked;

	public GameObject backgroundNormal, backgroundInActive;

	void Start()
	{
		levelNumber = int.Parse(gameObject.name);
		backgroundNormal.SetActive(true);
		backgroundInActive.SetActive(false);

		var levelReached = GlobalValue.LevelHighest;
		
		//if ((levelNumber <= levelReached))
		{
			TextLevel.text = levelNumber.ToString();
			Locked.SetActive(false);

			//Test purpose staf
            bool isInActive = true;


            var openLevel = levelReached + 1 >= levelNumber /*int.Parse(gameObject.name)*/;

            //Test purpose staf
            Locked.SetActive(isInActive);
            //	Locked.SetActive(!openLevel);

            //	bool isInActive = levelNumber == levelReached;


            backgroundNormal.SetActive(!isInActive);
			backgroundInActive.SetActive(isInActive);

            //GetComponent<Button>().interactable = openLevel;
            //Test purpose staf
            GetComponent<Button>().interactable = isInActive;

        }
        /*	else
            {
                TextLevel.gameObject.SetActive(false);
                Locked.SetActive(true);
                GetComponent<Button>().interactable = false;
            }*/
    }

	public void LoadScene()
	{
		GlobalValue.levelPlaying = levelNumber;
		HomeMenu.Instance.LoadLevel();
	}
}

