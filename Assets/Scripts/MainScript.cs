using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Xml;
using System.Collections.Generic;
using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

public class MainScript : MonoBehaviour {

	public Text m_Text;
	public Text m_textBestScore;
	public Text m_idCurrentQuestion;
	public UnityEngine.Sprite buttonGood, buttonBad, buttonNeutral;

	List<Dictionary<string,string>> m_lQuestions;
	Button[] m_Btns;
	List<int> m_nIds;
	List<int> m_nBannedIds;
	int m_nCurrentId;
	int m_nScore; 
	int m_nQuestionsCounter;

	bool m_bAnimationEffectAnswer;
	float m_fTimer, m_fRefTime, m_fWaintingTime;
	bool m_bCorrectAnswer;

	int m_nSavedBestScore;

	void Awake()
	{

	}

	void Start () {

		m_bCorrectAnswer = false;

		m_fWaintingTime = 0.5f;

		m_bAnimationEffectAnswer = false;

		m_nScore = 0;

		m_lQuestions = new List<Dictionary<string,string>>();

		LoadGameFromXml();

		m_nIds = new List<int> ();
		m_nBannedIds = new List<int> ();

		for (int i = 0; i < m_lQuestions.Count; ++i) {
			m_nIds.Add(i);
		}

		Shuffle(m_nIds);

		GameObject gh = GameObject.Find("Canvas");
		m_Btns =  gh.GetComponentsInChildren<Button>();

		m_nCurrentId = 0;
		m_Text.text = m_lQuestions[m_nIds[m_nCurrentId]]["value"];

		m_nQuestionsCounter = 1;
		m_idCurrentQuestion.text = 1 + " / " + m_lQuestions.Count;

		m_nSavedBestScore = 0;
		LoadBestScore ();

		m_textBestScore.text = "Best Score : " + m_nSavedBestScore;

		NextQuestion ();
	}

	void Update() {
		m_fTimer += Time.deltaTime;
		if (m_fTimer > (m_fRefTime + m_fWaintingTime) && m_bAnimationEffectAnswer)
		{
			m_bAnimationEffectAnswer = false;

			if ( m_bCorrectAnswer ) {
				++m_nScore;
				++m_nQuestionsCounter;
				++m_nCurrentId;
			} else {
				if (m_nScore > m_nSavedBestScore) { // if new best score
					m_nSavedBestScore = m_nScore;
					SaveBestScore();
				}

				ResetGame();
			}

			if ( m_nQuestionsCounter > m_lQuestions.Count )
			{
				ResetGame();
			}

			m_idCurrentQuestion.text = m_nQuestionsCounter + " / " + m_lQuestions.Count;

			for (int i = 0; i < 4; ++i)
			{
				UnityEngine.UI.Image buttonImage;
				buttonImage = m_Btns[i].GetComponent<UnityEngine.UI.Image>();
				buttonImage.overrideSprite = buttonNeutral;
			}

			NextQuestion();

		}
	}
	
	public void OnClickButton (Button p_clickedBtn) {
		UnityEngine.UI.Image buttonImage;
		if (!m_bAnimationEffectAnswer) {
			//if good answer
			if (p_clickedBtn.GetComponentInChildren<Text> ().text.CompareTo (m_lQuestions [m_nIds [m_nCurrentId]] ["response"]) == 0) { 
				buttonImage = p_clickedBtn.GetComponent<UnityEngine.UI.Image> ();
				buttonImage.overrideSprite = buttonGood;
				m_bCorrectAnswer = true;
			} else {
				buttonImage = p_clickedBtn.GetComponent<UnityEngine.UI.Image> ();
				buttonImage.overrideSprite = buttonBad;
				m_bCorrectAnswer = false;
			}
			m_fRefTime = Time.time;

			m_bAnimationEffectAnswer = true;
		}
	}
	
	public void LoadGameFromXml()
	{
		Dictionary<string,string> obj;

		TextAsset xmlData = new TextAsset();
		xmlData = (TextAsset)Resources.Load("data_en", typeof(TextAsset));
		if(xmlData != null)
		{
			XmlTextReader textReader = new XmlTextReader(new StringReader(xmlData.text));
			while (textReader.Read()) {
				switch(textReader.NodeType) {
					case XmlNodeType.Element: {
						switch(textReader.Name) {
							case "question": {              
								obj = new Dictionary<string,string> (); 
								obj.Add("value", textReader.GetAttribute("value"));
								obj.Add("response", textReader.GetAttribute("response"));
								m_lQuestions.Add(obj);
							}
							break;
						}
					}
					break;
				}
			}
		}
	}

	public void NextQuestion() {
		m_Text.text = m_lQuestions[m_nIds[m_nCurrentId]]["value"];

		//emplacement question
		System.Random rd = new System.Random();
		int idBtnGoodAswer = rd.Next(0, 4);
		string goodAnswer = "";
		string badAnswer = "";
		int nIdBadAnswer = 0;
		System.Random rd1 = new System.Random();
		m_nBannedIds.Add(m_nCurrentId);
		for (int i = 0; i < 4; ++i)
		{
			if ( i == idBtnGoodAswer ) 
			{
				goodAnswer = m_lQuestions [m_nIds [m_nCurrentId]] ["response"];
				m_Btns[i].GetComponentInChildren<Text>().text = goodAnswer;
			}
			else
			{
				nIdBadAnswer = rd1.Next(0, m_nIds.Count);
			
				while (IsBannedId(nIdBadAnswer)) 
				{
					nIdBadAnswer = rd1.Next(0, m_nIds.Count);
				}
				m_nBannedIds.Add(nIdBadAnswer);

				badAnswer = m_lQuestions [m_nIds [nIdBadAnswer]] ["response"];
				m_Btns[i].GetComponentInChildren<Text>().text = badAnswer;
			}
		}

		m_nBannedIds.Clear();
	} 

	public bool IsBannedId(int p_nIdToTest)
	{
		for (int i = 0; i < m_nBannedIds.Count; ++i) {
			if ( m_nBannedIds[i] == p_nIdToTest ) 
			{
				return true;
			}
		}
		return false;
	}
	
	public void Shuffle(List<int> list) {
		int counter = list.Count;
		System.Random rnd = new System.Random();
		while (counter > 1) {
			int k = (rnd.Next(0, counter) % counter);
			counter--;
			int value = list[k];
			list[k] = list[counter];
			list[counter] = value;
		}
	}

	public void ResetGame()
	{
		m_nCurrentId = 0;
		m_nQuestionsCounter = 1;

		m_nScore = 0;

		m_nIds.Clear();
		
		for (int i = 0; i < m_lQuestions.Count; ++i) {
			m_nIds.Add(i);
		}
		
		Shuffle(m_nIds);

		LoadBestScore ();
	}
	
	public void SaveBestScore()
	{
		PlayerPrefs.SetInt("BestScore", m_nSavedBestScore);
	}
	
	public void LoadBestScore()
	{
		m_nSavedBestScore = PlayerPrefs.GetInt("BestScore");
	}
}