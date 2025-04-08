using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using TMPro;

public class MicController : MonoBehaviour
{
    [SerializeField] private string m_DeviceName;
    private AudioClip m_AudioClip;
    private int m_LastAudioPos;
    private float m_AudioLevel;

    [SerializeField] private GameObject m_Cube;
    [SerializeField, Range(20, 200)] private float m_AmpGain = 50;
    [SerializeField, Range(0.002f, 1.0f)] private float m_Threshold = 0.05f;
    private Renderer m_CubeRenderer;

    [SerializeField] private TextMeshProUGUI textMesh; // カウントダウン用

    [SerializeField] private GameObject markMaru; // 丸の画像
    [SerializeField] private GameObject markBatsu; // バツの画像

    private string nextScene = "main"; // 次のシーン名
    private bool isListening = false; // マイク認識を制御するフラグ
    private bool isSuccess = false;   // 成功判定
    private bool isChecking = false;  // 叫びのチェック中フラグ

    // 正解・不正解のSE
    public AudioClip seMaru;
    public AudioClip seBatu;
    AudioSource audioSource;

    void Start()
    {
        // SEの設定
        audioSource = GetComponent<AudioSource>();
        m_CubeRenderer = m_Cube.GetComponent<Renderer>();
        markMaru.SetActive(false);
        markBatsu.SetActive(false);

        StartCoroutine(CountdownSequence());
    }

    IEnumerator CountdownSequence()
    {
        textMesh.text = "たすけをよぼう!!";
        yield return new WaitForSeconds(1.5f);

        for (int i = 3; i > 0; i--)
        {
            textMesh.text = i.ToString();
            yield return new WaitForSeconds(1);
        }

        textMesh.text = "さけべ!!";
        StartMicrophone();
        StartCoroutine(ScreamTimer());
    }

    void StartMicrophone()
    {
        string targetDevice = "";

        foreach (var device in Microphone.devices)
        {
            Debug.Log($"Device Name: {device}");
            if (device.Contains(m_DeviceName))
            {
                targetDevice = device;
            }
        }

        Debug.Log($"=== Device Set: {targetDevice} ===");
        m_AudioClip = Microphone.Start(targetDevice, true, 10, 48000);
        isListening = true;
        isChecking = true;
    }

    IEnumerator ScreamTimer()
    {
        yield return new WaitForSeconds(5);
        isChecking = false;
        textMesh.text = ""; // テキストを消す

        // マークを表示
        if (isSuccess)
        {
            markMaru.SetActive(true);

            // 正解のSEを再生
            audioSource.PlayOneShot(seMaru);
        }
        else
        {
            markBatsu.SetActive(true);

            // 不正解のSEを再生
            audioSource.PlayOneShot(seBatu);
        }

        // 1秒待ってシーン遷移
        yield return new WaitForSeconds(1);
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
    }

    void Update()
    {
        if (!isListening || !isChecking) return;

        float[] waveData = GetUpdatedAudio();
        if (waveData.Length == 0) return;

        m_AudioLevel = waveData.Average(Mathf.Abs);
        m_Cube.transform.localScale = new Vector3(1, 1 + m_AmpGain * m_AudioLevel, 1);

        if (m_AudioLevel > m_Threshold)
        {
            isSuccess = true;
        }
    }

    private float[] GetUpdatedAudio()
    {
        int nowAudioPos = Microphone.GetPosition(null);
        float[] waveData = Array.Empty<float>();

        if (m_LastAudioPos < nowAudioPos)
        {
            int audioCount = nowAudioPos - m_LastAudioPos;
            waveData = new float[audioCount];
            m_AudioClip.GetData(waveData, m_LastAudioPos);
        }
        else if (m_LastAudioPos > nowAudioPos)
        {
            int audioBuffer = m_AudioClip.samples * m_AudioClip.channels;
            int audioCount = audioBuffer - m_LastAudioPos;

            float[] wave1 = new float[audioCount];
            m_AudioClip.GetData(wave1, m_LastAudioPos);

            float[] wave2 = new float[nowAudioPos];
            if (nowAudioPos != 0)
            {
                m_AudioClip.GetData(wave2, 0);
            }

            waveData = new float[audioCount + nowAudioPos];
            wave1.CopyTo(waveData, 0);
            wave2.CopyTo(waveData, audioCount);
        }

        m_LastAudioPos = nowAudioPos;

        return waveData;
    }
}
