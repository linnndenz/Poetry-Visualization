using System;
using System.Collections;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

//Դ��https://blog.csdn.net/qq_21397217/article/details/119155513
//����Text_Poem-Viewport-Content��

public enum TypewriterState { Completed, Outputting, Interrupted }//���ֻ�Ч��״̬��

/// <summary>
/// ����TextMeshPro�Ĵ��ֻ�Ч�������
/// ��֪���⣺
///   1. FadeRange����0ʱ����ǿ�ƽ��ɼ��ַ���͸������Ϊ��ȫ��͸����
///      Ҫ�޸������⣬��Ҫ�ڿ�ʼ����ַ�ǰ��¼�����ַ���ԭʼ͸���ȣ�����ִ���ַ�����ʱ�����¼��ԭʼ͸���Ƚ��м��㡣
///   2. ����ɾ���ߡ��»��ߡ�����ɫ��Ч�����ı�����������ʾ��
///   3. ����ַ��Ĺ����иı�TextMeshPro�����RectTransform�������ᵼ���ı���ʾ�쳣��
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TextManager : MonoBehaviour
{
    /// �ַ�����ٶȣ�����/�룩��
    public byte OutputSpeed
    {
        get { return _outputSpeed; }
        set
        {
            _outputSpeed = value;
            CompleteOutput();
        }
    }

    /// �ַ�������Χ����������
    public byte FadeRange
    {
        get { return _fadeRange; }
        set
        {
            _fadeRange = value;
            CompleteOutput();
        }
    }

    /// <summary>
    /// ���ֻ�Ч��״̬��
    /// </summary>
    public TypewriterState State { get; private set; } = TypewriterState.Completed;


    [Tooltip("�ַ�����ٶȣ�����/�룩��")]
    [Range(1, 255)]
    [SerializeField]
    private byte _outputSpeed = 20;

    [Tooltip("�ַ�������Χ����������")]
    [Range(0, 50)]
    [SerializeField]
    private byte _fadeRange = 10;

    /// TextMeshPro�����
    private TMP_Text _textComponent;

    /// ��������ַ���Э�̡�
    private Coroutine _outputCoroutine;

    /// �ַ��������ʱ�Ļص���
    private Action<TypewriterState> _outputEndCallback;


    /// <summary>
    /// ������֡�
    /// </summary>
    /// <param name="text"></param>
    /// <param name="onOutputEnd"></param>
    public void OutputText(string text,Action<TypewriterState> onOutputEnd = null)
    {
        // �����ǰ����ִ���ַ�����������ж�
        if (State == TypewriterState.Outputting) {
            StopCoroutine(_outputCoroutine);

            State = TypewriterState.Interrupted;
            OnOutputEnd(false);
        }

        _textComponent.text = text;

        _outputEndCallback = onOutputEnd;

        // �������δ���ֱ��������
        if (!isActiveAndEnabled) {
            State = TypewriterState.Completed;
            OnOutputEnd(true);
            return;
        }

        // ��ʼ�µ��ַ����Э��
        if (FadeRange > 0) {
            _outputCoroutine = StartCoroutine(OutputCharactersFading());
        } else {
            _outputCoroutine = StartCoroutine(OutputCharactersNoFading());
        }
    }


    /// <summary>
    /// �Դ��е���Ч������ַ���Э�̡�
    /// </summary>
    /// <returns></returns>
    private IEnumerator OutputCharactersFading()
    {
        //�ӳ�һ֡ǿ��ˢ��***
        yield return new WaitForEndOfFrame();
        //GetComponent<ContentSizeFitter>().SetLayoutVertical();
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());//***

        State = TypewriterState.Outputting;

        // ȷ���ַ����ڿɼ�״̬
        var textInfo = _textComponent.textInfo;
        _textComponent.maxVisibleCharacters = textInfo.characterCount;
        _textComponent.ForceMeshUpdate();

        // û���ַ�ʱ��ֱ�ӽ������
        if (textInfo.characterCount == 0) {
            State = TypewriterState.Completed;
            OnOutputEnd(false);

            yield break;
        }

        // �Ƚ������ַ����õ�͸��״̬
        _textComponent.color = new Color32(0,0,0,0);
        //�ַ�̫�����bug���ĳ������ͨ��
        //for (int i = 0; i < textInfo.characterCount; i++) {
        //    SetCharacterAlpha(i, 0);
        //}

        // ��ʱ������ʾ�ַ�
        var timer = 0f;
        var interval = 1.0f / OutputSpeed;
        var headCharacterIndex = 0;
        while (State == TypewriterState.Outputting) {
            timer += Time.deltaTime;

            // �����ַ�������ɫ͸����
            var isFadeCompleted = true;
            var tailIndex = headCharacterIndex - FadeRange + 1;
            for (int i = headCharacterIndex; i > -1 && i >= tailIndex; i--) {
                // �������ɼ��ַ���������ܵ���ĳЩλ�õ��ַ���˸
                if (!textInfo.characterInfo[i].isVisible) {
                    continue;
                }

                var step = headCharacterIndex - i;
                var alpha = (byte)Mathf.Clamp((timer / interval + step) / FadeRange * 255, 0, 255);

                isFadeCompleted &= alpha == 255;
                SetCharacterAlpha(i, alpha);
            }

            _textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            // ����Ƿ�����ַ����
            if (timer >= interval) {
                if (headCharacterIndex < textInfo.characterCount - 1) {
                    timer = 0;
                    headCharacterIndex++;
                } else if (isFadeCompleted) {
                    State = TypewriterState.Completed;
                    OnOutputEnd(false);

                    yield break;
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// ������ڽ��еĴ��ֻ�Ч����������������ʾ������
    /// </summary>
    public void CompleteOutput()
    {
        if (State == TypewriterState.Outputting) {
            State = TypewriterState.Completed;
            StopCoroutine(_outputCoroutine);
            OnOutputEnd(true);
        }
    }


    //private void OnValidate()
    //{
    //    if (State == TypewriterState.Outputting) {
    //        OutputText(_textComponent.text);
    //    }
    //}

    private void Awake()
    {
        _textComponent = GetComponent<TMP_Text>();
        
        //������
        _textComponent.color = new Color32(0, 0, 0, 0);
    }

    private void OnDisable()
    {
        // �ж����
        if (State == TypewriterState.Outputting) {
            State = TypewriterState.Interrupted;
            StopCoroutine(_outputCoroutine);
            OnOutputEnd(true);
        }
    }

    /// <summary>
    /// �Բ�������Ч������ַ���Э�̡�
    /// </summary>
    /// <param name="skipFirstCharacter"></param>
    /// <returns></returns>
    private IEnumerator OutputCharactersNoFading(bool skipFirstCharacter = true)
    {
        State = TypewriterState.Outputting;

        // �����������ַ�
        _textComponent.maxVisibleCharacters = skipFirstCharacter ? 1 : 0;
        _textComponent.ForceMeshUpdate();

        // ��ʱ�������ʾ�ַ�
        var timer = 0f;
        var interval = 1.0f / OutputSpeed;
        var textInfo = _textComponent.textInfo;
        while (_textComponent.maxVisibleCharacters < textInfo.characterCount) {
            timer += Time.deltaTime;
            if (timer >= interval) {
                timer = 0;
                _textComponent.maxVisibleCharacters++;
            }

            yield return null;
        }

        // ������̽���
        State = TypewriterState.Completed;
        OnOutputEnd(false);
    }


    /// <summary>
    /// �����ַ��Ķ�����ɫAlphaֵ��
    /// </summary>
    /// <param name="index"></param>
    /// <param name="alpha"></param>
    private void SetCharacterAlpha(int index, byte alpha)
    {
        var materialIndex = _textComponent.textInfo.characterInfo[index].materialReferenceIndex;
        var vertexColors = _textComponent.textInfo.meshInfo[materialIndex].colors32;
        var vertexIndex = _textComponent.textInfo.characterInfo[index].vertexIndex;

        vertexColors[vertexIndex + 0].a = alpha;
        vertexColors[vertexIndex + 1].a = alpha;
        vertexColors[vertexIndex + 2].a = alpha;
        vertexColors[vertexIndex + 3].a = alpha;

        //newVertexColors[vertexIndex + 0] = (Color)newVertexColors[vertexIndex + 0] * ColorTint;
        //newVertexColors[vertexIndex + 1] = (Color)newVertexColors[vertexIndex + 1] * ColorTint;
        //newVertexColors[vertexIndex + 2] = (Color)newVertexColors[vertexIndex + 2] * ColorTint;
        //newVertexColors[vertexIndex + 3] = (Color)newVertexColors[vertexIndex + 3] * ColorTint;
    }

    public void SetAllCharacterAlpha(byte alpha)
    {
        for (int i = 0; i < _textComponent.textInfo.characterInfo.Length; i++) {
            var materialIndex = _textComponent.textInfo.characterInfo[i].materialReferenceIndex;
            var vertexColors = _textComponent.textInfo.meshInfo[materialIndex].colors32;
            var vertexIndex = _textComponent.textInfo.characterInfo[i].vertexIndex;

            vertexColors[vertexIndex + 0].a = alpha;
            vertexColors[vertexIndex + 1].a = alpha;
            vertexColors[vertexIndex + 2].a = alpha;
            vertexColors[vertexIndex + 3].a = alpha;

        }
    }

    /// <summary>
    /// ������������߼���
    /// </summary>
    /// <param name="isShowAllCharacters"></param>
    private void OnOutputEnd(bool isShowAllCharacters)
    {
        // ����Э��
        _outputCoroutine = null;

        // �������ַ���ʾ����
        if (isShowAllCharacters) {
            var textInfo = _textComponent.textInfo;
            for (int i = 0; i < textInfo.characterCount; i++) {
                SetCharacterAlpha(i, 255);
            }

            _textComponent.maxVisibleCharacters = textInfo.characterCount;
            _textComponent.ForceMeshUpdate();
        }

        // ���������ɻص�
        if (_outputEndCallback != null) {
            var temp = _outputEndCallback;
            _outputEndCallback = null;
            temp.Invoke(State);
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(TextManager))]
class TypewriterEditor : UnityEditor.Editor
{
    private TextManager Target => (TextManager)target;


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUI.BeginDisabledGroup(!Application.isPlaying || !Target.isActiveAndEnabled);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Restart")) {
            Target.OutputText(Target.GetComponent<TMP_Text>().text);
        }
        if (GUILayout.Button("Complete")) {
            Target.CompleteOutput();
        }
        GUILayout.EndHorizontal();
        UnityEditor.EditorGUI.EndDisabledGroup();
    }
}
#endif
