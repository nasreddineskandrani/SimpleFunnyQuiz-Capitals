using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Xml;
using System.Collections.Generic;
using System.IO;

public class MainScript : MonoBehaviour {

	public Text m_Text;
	public Text m_Score;
	public UnityEngine.Sprite buttonGood, buttonBad, buttonNeutral;

	List<Dictionary<string,string>> m_lQuestions;
	Button[] m_Btns;
	List<int> m_nIds;
	List<int> m_nBannedIds;
	int m_nCurrentId;
	int m_nScore; 

	bool m_bAnimationEffectAnswer;
	float m_fTimer, m_fRefTime, m_fWaintingTime;
	bool m_bCorrectAnswer;

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

		NextQuestion ();
	}

	void Update() {
		m_fTimer += Time.deltaTime;
		if (m_fTimer > (m_fRefTime + m_fWaintingTime) && m_bAnimationEffectAnswer)
		{
			m_bAnimationEffectAnswer = false;

			if ( m_bCorrectAnswer ) {
				++m_nScore;
				m_Score.text = "Score : " + m_nScore;
			} else {
				m_nScore = 0;
				m_Score.text = "Score : " + m_nScore;
			}
			
			++m_nCurrentId;
			if (m_nCurrentId == m_nIds.Count)
			{
				Shuffle(m_nIds);
				m_nCurrentId = 0;
			}

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

	public void LoadGameFromXml()
	{	
		XmlDocument xmlDoc = new XmlDocument ();
		xmlDoc.Load("./Assets/Data/data_en.xml"); 
		XmlNodeList questionsList = xmlDoc.GetElementsByTagName("question"); 

		Dictionary<string,string> obj;
		foreach (XmlNode question in questionsList) { 
			obj = new Dictionary<string,string> (); 
			obj.Add("value",question.Attributes.GetNamedItem("value").Value);
			obj.Add("response",question.Attributes.GetNamedItem("response").Value);
			m_lQuestions.Add(obj);
		}
	}

	public void NextQuestion() {

		m_Text.text = m_lQuestions[m_nIds[m_nCurrentId]]["value"];

		//emplacement question
		System.Random rd = new System.Random();
		int idBtnGoodAswer = rd.Next(0, 4);
		string goodAnswer = "";
		int nIdAnswer = 0;
		System.Random rd1 = new System.Random();
		for (int i = 0; i < 4; ++i)
		{
			if ( i == idBtnGoodAswer ) 
			{
				goodAnswer = m_lQuestions [m_nIds [m_nCurrentId]] ["response"];
				m_Btns[i].GetComponentInChildren<Text>().text = goodAnswer;
			}
			else
			{
				nIdAnswer = rd1.Next(0, m_nIds.Count);
			
				while (nIdAnswer == m_nCurrentId && IsBannedId(nIdAnswer)) 
				{
					nIdAnswer = rd1.Next(0, m_nIds.Count);
					m_nBannedIds.Add(nIdAnswer);
				}

				goodAnswer = m_lQuestions [m_nIds [nIdAnswer]] ["response"];
				m_Btns[i].GetComponentInChildren<Text>().text = goodAnswer;
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
		int n = list.Count;
		System.Random rnd = new System.Random();
		while (n > 1) {
			int k = (rnd.Next(0, n) % n);
			n--;
			int value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}
}