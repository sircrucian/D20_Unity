using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _modifierInputField;
    [SerializeField] private TMP_InputField _requiredRollValueInputField;
    [SerializeField] private TMP_Text _checkResult;
    [SerializeField] private TMP_Text _currentValue;
    [SerializeField] private Button _rollButton;
    [SerializeField] private Toggle _toggleModificator1;
    [SerializeField] private Toggle _toggleModificator2;

    [SerializeField] private GameObject _dicePrefab;

    [Tooltip("Rotation speed.")] [SerializeField]
    private float _rotationSpeed = 8f;

    [Tooltip("Rotation duration.")] [SerializeField]
    private float _rotationDuration = 2f;

    [Tooltip("Duration of rotation from random to rolled result.")] [SerializeField]
    private float _switchTime = 0.5f;

    [Tooltip("Radius of dice animation path.")] [SerializeField]
    private float _circleRadius = 10f;

    private bool _addMode = false;
    private bool _trollMode = false;
    private int _modifierValue;
    
    /// <summary>
    /// Rotation to roll results from 1 to 20. 
    /// </summary>
    private readonly Quaternion[] _positions = 
    {
        Quaternion.Euler(new Vector3(75, -258, -277)),
        Quaternion.Euler(new Vector3(-30, -104, 121)),
        Quaternion.Euler(new Vector3(200, -264, -574)),
        Quaternion.Euler(new Vector3(-13, -34, 131)),
        Quaternion.Euler(new Vector3(-147, -76, -188)),
        Quaternion.Euler(new Vector3(-153, -146, -331)),
        Quaternion.Euler(new Vector3(-151, 4, 153)),
        Quaternion.Euler(new Vector3(-152, -70, 21)),
        Quaternion.Euler(new Vector3(35, 14, 45)),
        Quaternion.Euler(new Vector3(216, -1073, -423)),
        Quaternion.Euler(new Vector3(144, -172, -220)),
        Quaternion.Euler(new Vector3(215, 0, 10)),
        Quaternion.Euler(new Vector3(161, -103, -155)),
        Quaternion.Euler(new Vector3(148, -177, -12)),
        Quaternion.Euler(new Vector3(167, -35, -174)),
        Quaternion.Euler(new Vector3(172, -103, -11)),
        Quaternion.Euler(new Vector3(149, -326, -239)),
        Quaternion.Euler(new Vector3(150, 110, -19)),
        Quaternion.Euler(new Vector3(147, 105, 121)),
        Quaternion.Euler(new Vector3(266, 181, 161))
    };

    private int _randomDiceValue;
    private int _inputCheckValue;

    private Coroutine _diceAnimationCoroutine;
    private Coroutine _diceToResultCoroutine;

    private Vector3 _startPosition;
    
    private GameObject _diceInstance;
    
    private void Awake()
    {
        _rollButton.onClick.AddListener(HandleOnClick);
        _requiredRollValueInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
        _modifierInputField.onValueChanged.AddListener(OnModifierFieldValueChanged);
        _toggleModificator1.onValueChanged.AddListener(AddModificatorSwitch);
        _toggleModificator2.onValueChanged.AddListener(TrollModificatorSwitch);

        if (_diceInstance == null)
        {
            _diceInstance = Instantiate(_dicePrefab);
            _diceInstance.transform.position = new Vector3(0, 0, -4);
        }

        _diceInstance.transform.localScale += new Vector3(1500.0f, 1500.0f, 1500.0f);
        _diceInstance.transform.localRotation = _positions[6];
        _startPosition = _diceInstance.transform.position;
    }

    private void OnInputFieldValueChanged(string value)
    {
        if (int.TryParse(value, out int result))
        {
            _inputCheckValue = Mathf.Clamp(result, 1, 20);
        }
        else
        {
            _inputCheckValue = 1;
            _requiredRollValueInputField.text = _inputCheckValue.ToString();
        }
    }
    
    private void OnModifierFieldValueChanged(string value)
    {
        if (int.TryParse(value, out int result))
        {
            _modifierValue = Mathf.Clamp(result, 1, 20);
        }
        else
        {
            _modifierValue = 1;
            _modifierInputField.text = _modifierValue.ToString();
        }
    }

    /// <summary>
    /// Modifiers disable each other. The first modifier adds a number from 1 to 20 to the rolled value
    /// </summary>
    private void AddModificatorSwitch(bool isOn)
    {
        if (_trollMode)
        {
            _toggleModificator2.isOn = false;
        }
        _addMode = isOn;
    }

    /// <summary>
    /// The value on the dice will not appear more than necessary, which means the check will not be successful.
    /// </summary>
    private void TrollModificatorSwitch(bool isOn)
    {
        if (_addMode)
        {
            _toggleModificator1.isOn = false;
        }
        _trollMode = isOn;
    }
    
    private void OnDestroy()
    {
        _rollButton.onClick.RemoveListener(HandleOnClick);
    }

    private void DoCheckResult()
    {
        if (_addMode)
        {
            _randomDiceValue =+ _modifierValue;
        }
        if (_randomDiceValue > _inputCheckValue)
            _checkResult.text = "Успех";
        else
            _checkResult.text = "Провал";
    }

    private void HandleOnClick()
    {
        _randomDiceValue = Random.Range(0, _positions.Length);

        if (_trollMode)
        {
            _randomDiceValue =  Random.Range(0, _inputCheckValue);
        }
        else
        {
            _requiredRollValueInputField.text = _inputCheckValue.ToString();
        }

        if (_diceAnimationCoroutine != null)
        {
            StopCoroutine(_diceAnimationCoroutine);
        }

        _diceAnimationCoroutine = StartCoroutine(StartAnimationRoll());
    }

    /// <summary>
    /// First animation, rotating and moving in a circle for a few seconds
    /// </summary>
    private IEnumerator StartAnimationRoll()
    {
        var timer = 0.0f;
        
        while (timer <= _rotationDuration)
        {
            var angle = Mathf.Lerp(0f, 360f, timer / _rotationDuration);
            var x = _startPosition.x + _circleRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
            var y = _startPosition.y + _circleRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
            _diceInstance.transform.position = new Vector3(x, y, _startPosition.z);
            _diceInstance.transform.Rotate(Vector3.right * 2, _rotationSpeed * 200 * Time.deltaTime);

            timer += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        
        yield return SwitchRotation();
    }

    /// <summary>
    /// Second animation, rotation to the desired position
    /// </summary>
    private IEnumerator SwitchRotation()
    {
        var startTime = Time.time;
        var currentRotation = _diceInstance.transform.rotation;
        var nextRotation = _positions[_randomDiceValue];
        
        while (Time.time < startTime + _switchTime)
        {
            var step = (Time.time - startTime) / _switchTime;
            _diceInstance.transform.rotation = Quaternion.Slerp(currentRotation, nextRotation, step);
            yield return new WaitForFixedUpdate();
        }
        
        _currentValue.text = (_randomDiceValue + 1).ToString();
        DoCheckResult();
    }
}
