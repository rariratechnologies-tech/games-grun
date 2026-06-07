using UnityEngine;

public class PrivacyButtonHandler : MonoBehaviour
{
    public void OpenPrivacyPolicy()
    {
        Application.OpenURL("https://rarira.com/littleganesha/privacy/");
    }
}
