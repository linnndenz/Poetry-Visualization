using System;
using System.Collections;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

//源自https://blog.csdn.net/qq_21397217/article/details/119155513
//挂在Text_Poem-Viewport-Content上

public enum TypewriterState { Completed, Outputting, Interrupted }//打字机效果状态。

/// <summary>
/// 用于TextMeshPro的打字机效果组件。
/// 已知问题：
///   1. FadeRange大于0时，会强制将可见字符的透明度设为完全不透明。
///      要修复此问题，需要在开始输出字符前记录所有字符的原始透明度，并在执行字符淡化时代入记录的原始透明度进行计算。
///   2. 带有删除线、下划线、背景色等效果的文本不能正常显示。
///   3. 输出字符的过程中改变TextMeshPro组件的RectTransform参数，会导致文本显示异常。
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TextManager : MonoBehaviour
{
    /// 字符输出速度（字数/秒）。
    public byte OutputSpeed
    {
        get { return _outputSpeed; }
        set
        {
            _outputSpeed = value;
            CompleteOutput();
        }
    }

    /// 字符淡化范围（字数）。
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
    /// 打字机效果状态。
    /// </summary>
    public TypewriterState State { get; private set; } = TypewriterState.Completed;


    [Tooltip("字符输出速度（字数/秒）。")]
    [Range(1, 255)]
    [SerializeField]
    private byte _outputSpeed = 20;

    [Tooltip("字符淡化范围（字数）。")]
    [Range(0, 50)]
    [SerializeField]
    private byte _fadeRange = 10;

    /// TextMeshPro组件。
    private TMP_Text _textComponent;

    /// 用于输出字符的协程。
    private Coroutine _outputCoroutine;

    /// 字符输出结束时的回调。
    private Action<TypewriterState> _outputEndCallback;


    /// <summary>
    /// 输出文字。
    /// </summary>
    /// <param name="text"></param>
    /// <param name="onOutputEnd"></param>
    public void OutputText(string text,Action<TypewriterState> onOutputEnd = null)
    {
        // 如果当前正在执行字符输出，将其中断
        if (State == TypewriterState.Outputting) {
            StopCoroutine(_outputCoroutine);

            State = TypewriterState.Interrupted;
            OnOutputEnd(false);
        }

        _textComponent.text = text;

        _outputEndCallback = onOutputEnd;

        // 如果对象未激活，直接完成输出
        if (!isActiveAndEnabled) {
            State = TypewriterState.Completed;
            OnOutputEnd(true);
            return;
        }

        // 开始新的字符输出协程
        if (FadeRange > 0) {
            _outputCoroutine = StartCoroutine(OutputCharactersFading());
        } else {
            _outputCoroutine = StartCoroutine(OutputCharactersNoFading());
        }
    }


    /// <summary>
    /// 以带有淡入效果输出字符的协程。
    /// </summary>
    /// <returns></returns>
    private IEnumerator OutputCharactersFading()
    {
        //延迟一帧强制刷新***
        yield return new WaitForEndOfFrame();
        //GetComponent<ContentSizeFitter>().SetLayoutVertical();
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());//***

        State = TypewriterState.Outputting;

        // 确保字符处于可见状态
        var textInfo = _textComponent.textInfo;
        _textComponent.maxVisibleCharacters = textInfo.characterCount;
        _textComponent.ForceMeshUpdate();

        // 没有字符时，直接结束输出
        if (textInfo.characterCount == 0) {
            State = TypewriterState.Completed;
            OnOutputEnd(false);

            yield break;
        }

        // 先将所有字符设置到透明状态
        _textComponent.color = new Color32(0,0,0,0);
        //字符太多会有bug，改成上面可通用
        //for (int i = 0; i < textInfo.characterCount; i++) {
        //    SetCharacterAlpha(i, 0);
        //}

        // 按时间逐渐显示字符
        var timer = 0f;
        var interval = 1.0f / OutputSpeed;
        var headCharacterIndex = 0;
        while (State == TypewriterState.Outputting) {
            timer += Time.deltaTime;

            // 计算字符顶点颜色透明度
            var isFadeCompleted = true;
            var tailIndex = headCharacterIndex - FadeRange + 1;
            for (int i = headCharacterIndex; i > -1 && i >= tailIndex; i--) {
                // 不处理不可见字符，否则可能导致某些位置的字符闪烁
                if (!textInfo.characterInfo[i].isVisible) {
                    continue;
                }

                var step = headCharacterIndex - i;
                var alpha = (byte)Mathf.Clamp((timer / interval + step) / FadeRange * 255, 0, 255);

                isFadeCompleted &= alpha == 255;
                SetCharacterAlpha(i, alpha);
            }

            _textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            // 检查是否完成字符输出
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
    /// 完成正在进行的打字机效果，将所有文字显示出来。
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
        
        //先隐藏
        _textComponent.color = new Color32(0, 0, 0, 0);
    }

    private void OnDisable()
    {
        // 中断输出
        if (State == TypewriterState.Outputting) {
            State = TypewriterState.Interrupted;
            StopCoroutine(_outputCoroutine);
            OnOutputEnd(true);
        }
    }

    /// <summary>
    /// 以不带淡入效果输出字符的协程。
    /// </summary>
    /// <param name="skipFirstCharacter"></param>
    /// <returns></returns>
    private IEnumerator OutputCharactersNoFading(bool skipFirstCharacter = true)
    {
        State = TypewriterState.Outputting;

        // 先隐藏所有字符
        _textComponent.maxVisibleCharacters = skipFirstCharacter ? 1 : 0;
        _textComponent.ForceMeshUpdate();

        // 按时间逐个显示字符
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

        // 输出过程结束
        State = TypewriterState.Completed;
        OnOutputEnd(false);
    }


    /// <summary>
    /// 设置字符的顶点颜色Alpha值。
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
    /// 处理输出结束逻辑。
    /// </summary>
    /// <param name="isShowAllCharacters"></param>
    private void OnOutputEnd(bool isShowAllCharacters)
    {
        // 清理协程
        _outputCoroutine = null;

        // 将所有字符显示出来
        if (isShowAllCharacters) {
            var textInfo = _textComponent.textInfo;
            for (int i = 0; i < textInfo.characterCount; i++) {
                SetCharacterAlpha(i, 255);
            }

            _textComponent.maxVisibleCharacters = textInfo.characterCount;
            _textComponent.ForceMeshUpdate();
        }

        // 触发输出完成回调
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
