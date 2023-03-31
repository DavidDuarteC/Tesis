using System;
using System.Collections.Generic;
using UnityEngine;
public class QuizManager : MonoBehaviour, ISavable
{
    //ref to the QuizGameUI script
    [SerializeField] QuizGameUI quizGameUI;
    //ref to the scriptableobject file
    [SerializeField] List<DataQuiz> quizDataList;
    [SerializeField] float timeInSeconds;

    private string currentCategory = "";
    private int correctAnswerCount = 0;
    //questions data
    private List<Question> questions;
    //current question data
    private Question selectedQuetion = new Question();
    private int gameScore;
    private int lifesRemaining;
    private float currentTime;
    private QuizDataScriptable dataScriptable;

    private GameStatus gameStatus = GameStatus.NEXT;

    public GameStatus GameStatus { get { return gameStatus; } }

    public List<DataQuiz> QuizData { get => quizDataList; set { quizDataList = value; } }

    public static QuizManager i;

    //private void Awake()
    //{
    //    foreach (var data in quizDataList)
    //    {
    //        data.isComplete = false;
    //    }
    //}
    private void Start()
    {
        i = this;
    }

    public void StartGame(int categoryIndex, string category)
    {
        currentCategory = category;
        correctAnswerCount = 0;
        gameScore = 0;
        lifesRemaining = 1;
        currentTime = timeInSeconds;
        //set the questions data
        questions = new List<Question>();
        dataScriptable = quizDataList[categoryIndex].quiz;
        questions.AddRange(dataScriptable.questions);
        //select the question
        SelectQuestion();
        gameStatus = GameStatus.PLAYING;
    }

    /// <summary>
    /// Method used to randomly select the question form questions data
    /// </summary>
    private void SelectQuestion()
    {
        //get the random number
        int val = UnityEngine.Random.Range(0, questions.Count);
        //set the selectedQuetion
        selectedQuetion = questions[val];
        //send the question to quizGameUI
        quizGameUI.SetQuestion(selectedQuetion);

        questions.RemoveAt(val);
    }

    private void Update()
    {
        if (gameStatus == GameStatus.PLAYING)
        {
            currentTime -= Time.deltaTime;
            SetTime(currentTime);
        }
    }

    void SetTime(float value)
    {
        TimeSpan time = TimeSpan.FromSeconds(currentTime);                       //set the time value
        quizGameUI.TimerText.text = time.ToString("mm':'ss");   //convert time to Time format

        if (currentTime <= 0)
        {
            //Game Over
            GameEnd();
        }
    }

    /// <summary>
    /// Method called to check the answer is correct or not
    /// </summary>
    /// <param name="selectedOption">answer string</param>
    /// <returns></returns>
    public bool Answer(string selectedOption)
    {
        //set default to false
        bool correct = false;
        //if selected answer is similar to the correctAns
        if (selectedQuetion.correctAns == selectedOption)
        {
            //Yes, Ans is correct
            correctAnswerCount++;
            correct = true;
            gameScore += 50;
            quizGameUI.ScoreText.text = "Score:" + gameScore;
            StressLevel.i.LostMoreTimeRow = false;
        }
        else
        {
            //No, Ans is wrong
            //Reduce Life
            lifesRemaining--;
            quizGameUI.ReduceLife(lifesRemaining);
            StressLevel.i.AddLevel(StressLevel.i.LostMoreTimeRow ? StressLevel.i.LoseMultiply * 2 : StressLevel.i.LoseMultiply);
            if (lifesRemaining == 0)
            {
                GameEnd();
            }
        }

        if (gameStatus == GameStatus.PLAYING)
        {
            if (questions.Count > 0)
            {
                //call SelectQuestion method again after 1s
                Invoke("SelectQuestion", 0.4f);
            }
            else
            {
                GameEnd();
            }
        }
        //return the value of correct bool
        return correct;
    }

    private void GameEnd()
    {
        gameStatus = GameStatus.NEXT;
        quizGameUI.GameOverPanel.SetActive(true);

        //fi you want to save only the highest score then compare the current score with saved score and if more save the new score
        //eg:- if correctAnswerCount > PlayerPrefs.GetInt(currentCategory) then call below line

        //Save the score
        if(correctAnswerCount == dataScriptable.questions.Count)
        {
            for (int i = 0; i < quizDataList.Count; i++)
            {
                if (quizDataList[i].quiz.categoryName == currentCategory)
                {
                    quizDataList[i].isComplete = true;
                }
            }
        }
        else
        {
            PlayerPrefs.SetInt(currentCategory, correctAnswerCount); //save the score for this category
        }
            
        //QuizGameUI.i.ScrollHolder().GetComponents<CategoryBtnScript>().ConvertTo
    }

    public object CaptureState()
    {
        var saveData = new List<SaveDataQuiz>();

        for (int i = 0; i < quizDataList.Count; i++)
        {
            SaveDataQuiz save = new SaveDataQuiz(); // Crear una nueva instancia aquí
            save.category = quizDataList[i].quiz.categoryName;
            save.complete = quizDataList[i].isComplete;
            saveData.Add(save);
        }
        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as List<SaveDataQuiz>;
        //Debug.Log("null data");
        if (saveData != null)
        {
            //Debug.Log(saveData.ToArray());
            for (int i = 0; i < saveData.Count; i++)
            {
                for (int j = 0; j < quizDataList.Count; j++)
                {
                    // Intercambia el uso de 'i' y 'j' aquí
                    if (quizDataList[j].quiz.categoryName == saveData[i].category)
                        quizDataList[j].isComplete = saveData[i].complete;
                }
            }
        }
    }

}

//Datastructure for storeing the quetions data
[System.Serializable]
public class Question
{
    public string questionInfo;         //question text
    public QuestionType questionType;   //type
    public Sprite questionImage;        //image for Image Type
    public AudioClip audioClip;         //audio for audio type
    public UnityEngine.Video.VideoClip videoClip;   //video for video type
    public List<string> options;        //options to select
    public string correctAns;           //correct option
    public string urlVideo;
}

[System.Serializable]
public enum QuestionType
{
    TEXT,
    IMAGE,
    AUDIO,
    VIDEO
}

[SerializeField]
public enum GameStatus
{
    PLAYING,
    NEXT
}

[Serializable]
public class DataQuiz
{
    public QuizDataScriptable quiz;
    public bool isComplete;
}

[Serializable]
public class SaveDataQuiz
{
    public string category;
    public bool complete;
}