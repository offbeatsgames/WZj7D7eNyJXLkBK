using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MapControllerUI : MonoBehaviour {
//	public Transform BlockLevel;
	public RectTransform BlockLevel;
	public int howManyBlocks = 3;
	public float step = 720f;
    public int levelPerBlock = 10;
	public Image[] Dots;
	private float newPosX = 0;

	int currentPos = 0;
	public AudioClip music;

    public Button btnNext, btnPre;

    void OnEnable()
    {
        SoundManager.PlayMusic(music);
        Debug.LogWarning("ON ENALBE");

        SetCurrentWorld(Mathf.Clamp((GlobalValue.LevelHighest / levelPerBlock) + 1, 0, howManyBlocks));
    }
   
    void Start () {
        SetDots();
    }

    void SetDots()
    {
        foreach(var obj in Dots)
        {
            obj.color = new Color(1, 1, 1, 0.5f);
            obj.rectTransform.sizeDelta = new Vector2(28, 28);
        }
        
        Dots[currentPos].color = Color.yellow;
        Dots[currentPos].rectTransform.sizeDelta = new Vector2(38, 38);

        btnNext.interactable = currentPos < howManyBlocks - 1;
        btnPre.interactable = currentPos > 0;
    }
    
    void OnDisable()
    {
        if (SoundManager.Instance)
            SoundManager.PlayMusic(SoundManager.Instance.musicsGame);
    }

    public void SetCurrentWorld(int world)
    {
        currentPos = (world - 1);
        newPosX = 0;
        newPosX -= step * (world - 1);
        newPosX = Mathf.Clamp(newPosX, -step * (howManyBlocks - 1), 0);

        SetMapPosition();
        SetDots();
    }

    public void SetMapPosition()
    {
        BlockLevel.anchoredPosition = new Vector2(newPosX, BlockLevel.anchoredPosition.y);
    }

    bool allowPressButton = true;
    public void Next()
    {
        if (allowPressButton)
        {
            StartCoroutine(NextCo());
        }
    }

    IEnumerator NextCo()
    {
        allowPressButton = false;

        SoundManager.Click();

        if (newPosX != (-step * (howManyBlocks - 1)))
        {
            currentPos++;

            newPosX -= step;
            newPosX = Mathf.Clamp(newPosX, -step * (howManyBlocks - 1), 0);
            
        }
        else
        {
            allowPressButton = true;
            yield break;

            //currentPos = 0;

            //newPosX = 0;
            //newPosX = Mathf.Clamp(newPosX, -step * (howManyBlocks - 1), 0);


        }
        yield return new WaitForSeconds(0.1f);
        SetMapPosition();

        SetDots();


        allowPressButton = true;

    }

    public void Pre()
    {
        if (allowPressButton)
        {
            StartCoroutine(PreCo());
        }
    }

    IEnumerator PreCo()
    {
        allowPressButton = false;
        SoundManager.Click();
        if (newPosX != 0)
        {
            currentPos--;

            newPosX += step;
            newPosX = Mathf.Clamp(newPosX, -step * (howManyBlocks - 1), 0);


        }
        else
        {
            allowPressButton = true;
            yield break;
            //currentPos = howManyBlocks - 1;

            //newPosX = -999999;
            //newPosX = Mathf.Clamp(newPosX, -step * (howManyBlocks - 1), 0);

        }


        yield return new WaitForSeconds(0.1f);
        SetMapPosition();

        SetDots();


        allowPressButton = true;

    }

	public void UnlockAllLevels(){
		GlobalValue.LevelHighest = (GlobalValue.LevelHighest + 1000);
		UnityEngine.SceneManagement.SceneManager.LoadScene (UnityEngine.SceneManagement.SceneManager.GetActiveScene ().buildIndex);
		SoundManager.Click ();
	}
}
