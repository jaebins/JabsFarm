using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    // 로그인 관련 인스턴스
    static DatabaseReference db;

    // UI
    public TMP_InputField input_id;
    public TMP_InputField input_pw;

    // 정보
    public string userId;

    public void Login()
    {
        db = FirebaseDatabase.DefaultInstance.RootReference;

        db.Child("user").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("실패");
            }
            else{
                DataSnapshot snapshot = task.Result;

                if (snapshot.Child(input_id.text).Value != null && 
                    snapshot.Child(input_id.text).Value.Equals(input_pw.text)) // 아이디, 비번 일치
                {
                    userId = input_id.text;

                    Debug.Log("일치");
                    DontDestroyOnLoad(gameObject);
                    SceneManager.LoadScene(1);
                }
                else
                {
                    Debug.Log("틀림");
                }
            }
        });
    }

    static void WriteData()
    {
        db = FirebaseDatabase.DefaultInstance.RootReference;

        InputUser("12", "12");
        InputUser("34", "34");
        InputUser("56", "56");

        Debug.Log("작성완료");
    }

    public static void InputUser(string userId, string userPw)
    {
        db.Child("user").Child(userId).Push();
        db.Child("user").Child(userId).SetValueAsync(userPw);
    }
}
