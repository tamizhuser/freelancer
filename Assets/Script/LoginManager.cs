using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using SimpleJSON;
using System;
using UnityEngine.UI;
using System.Text.RegularExpressions;


public class LoginManager : MonoBehaviour
{

    [Header("Login Container")]
    public GameObject LoginPanel;
    public TMP_InputField _emailIdITF;
    [SerializeField]
    string _baseUrl = "https://api-dev.startar.in/";
    [SerializeField]
    string _otp_token;      
    [SerializeField]
    string _otp;
    public TextMeshProUGUI loginLog;


    [Header("OTP Container")]
    public GameObject OTPPanel;
    public TextMeshProUGUI wesendTxt;
    public TMP_InputField _otpITF;
    public TextMeshProUGUI _timeTxt;
    public Button ResendBtn;
    bool otp_expiry;
    public TextMeshProUGUI otpLog;
    

    public void LoginFunc()
    {
        string email = _emailIdITF.text;
        Regex regex = new Regex(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",RegexOptions.CultureInvariant | RegexOptions.Singleline);
        bool isValidEmail = regex.IsMatch(email);
        if (isValidEmail)
        {
            loginLog.text = "";
            StartCoroutine(LoginAPICall("email", _emailIdITF.text, true, "login"));
        }
        else
        {
            loginLog.text = "<color=red>The email is invalid";

        }
    }

    IEnumerator LoginAPICall(string ver_method,string email,bool checkRegUser,string otp_form_action)
    {
        WWWForm form = new WWWForm();
        form.AddField("ver_method", ver_method);
        form.AddField("email", email);
        form.AddField("checkRegUser", checkRegUser.ToString());
        form.AddField("otp_form_action", otp_form_action);

        using (UnityWebRequest www = UnityWebRequest.Post(_baseUrl + "api/v1/customer/verifyOTP", form))
        {
            yield return www.SendWebRequest();

            if(www.result==UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                JSONNode jsonArray = JSON.Parse (www.downloadHandler.text) as JSONNode;
                Debug.Log(jsonArray);
                Debug.Log(jsonArray["succ"]);
                if(bool.Parse(jsonArray["succ"]))
                {
                    //Otp Panel
                    _otp_token = jsonArray["otp_token"];
                    _otp = jsonArray["OTP"];
                    OTPPanel.SetActive(true);
                    LoginPanel.SetActive(false);
                    ResendBtn.interactable = false;
                    wesendTxt.text = "We have sent an email on your mail id\n" + "webdev.anup@gmail.com";
                    StartCoroutine(ResendEnable());
                    StartCoroutine(OTPExpiry());

                }
                else
                {
                    loginLog.text = "<color=red>Email Id Doesn't Exsit";
                    Debug.Log("Your Email ID doesn't exit");
                }
            }
        }
       
    }
    public void OtpFunc()
    {
        if(!otp_expiry)
        {
            if (_otp == _otpITF.text)
            {
                StartCoroutine(OtpAPICall(_otp_token, true, true, true, _otpITF.text));
                otpLog.text = "<color=green>Success";
            }
            else
            {
                otpLog.text = "<color=red>OTP is Wrong";

            }
        }
        else
        {
            otpLog.text = "<color=red>OTP Expired.Resend OTP";
        }
       
    }
    IEnumerator OtpAPICall(string otp_token, bool upsert, bool auto_login, bool get_user_details, string otp)
    {
        WWWForm form = new WWWForm();
        form.AddField("otp_token", otp_token);
        form.AddField("upsert", upsert.ToString());
        form.AddField("auto_login", auto_login.ToString());
        form.AddField("get_user_details", get_user_details.ToString());
        form.AddField("otp", otp);

        using (UnityWebRequest www = UnityWebRequest.Post(_baseUrl + "api/v1/customer/reqOTP", form))
        {
            yield return www.SendWebRequest();

            if(www.result==UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                JSONNode jsonArray = JSON.Parse (www.downloadHandler.text) as JSONNode;
                Debug.Log(jsonArray);
                Debug.Log(jsonArray["succ"]);
                if (bool.Parse(jsonArray["succ"]))
                {
                    //Otp Panel
                    _otp_token = jsonArray["otp_token"];
                    _otp = jsonArray["OTP"];
                }
                else
                {
                    Debug.Log("Your Email ID doesn't exit");
                }
            }
        }
       
    }

    IEnumerator ResendEnable()
    {
        for (int i = 1; i <=120; i++)
        {
            yield return new WaitForSeconds(1);
           
            TimeSpan time = TimeSpan.FromSeconds(120-i);
           
            _timeTxt.text = time.ToString(@"mm\:ss")+" left";
        }
        ResendBtn.interactable = true;
    }

    IEnumerator OTPExpiry()
    {
        for (int i = 1; i <= 600; i++)
        {
            yield return new WaitForSeconds(1);
        }
        otp_expiry = true;
    }

    public void CloseFun()
    {
        _otp_token = null;
        _otp = null;
        _emailIdITF.text = null;
        _otpITF.text = null;
        LoginPanel.SetActive(true);

        OTPPanel.SetActive(false);
    }
}
