using System;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class DFlexElement : MonoBehaviour, IFlexElement
{
    public enum ETypeSize
    {
        Fixed,
        Fill,
        Component,
        Layout
    }

    [Header("Settings")] [SerializeField] private bool _skipLayout;

    [SerializeField] private bool _checkGameObjectEnable;

    [Header("Size")] [SerializeField] private float _height =1;

    [SerializeField] private ETypeSize _heightType = ETypeSize.Component;
    [SerializeField] private float _width = 1;
    [SerializeField] private ETypeSize _widthType = ETypeSize.Component;

    [Header("Min/Max")] [SerializeField] private float _minHeight;

    [SerializeField] private float _minWidth;
    [SerializeField] private float _maxHeight;
    [SerializeField] private float _maxWidth;

    [Header("Margin")] [SerializeField] private float _leftMargin;

    [SerializeField] private float _rightMargin;
    [SerializeField] private float _topMargin;
    [SerializeField] private float _bottomMargin;

    private DFlexLayout _layout;
    private float _oldBottomMargin;

    private float _oldHeight;
    private ETypeSize _oldHeightType;

    private float _oldLeftMargin;
    private Vector3 _oldLocalPos;
    private float _oldMaxHeight;
    private float _oldMaxWidth;

    private float _oldMinHeight;
    private float _oldMinWidth;

    private Transform _oldParent;
    private float _oldRightMargin;
    private Vector2 _oldSize;

    private bool _oldSkipLayout;
    private float _oldTopMargin;
    private float _oldWidth;
    private ETypeSize _oldWidthType;

    private RectTransform _rect;

    public ETypeSize HeightType => _heightType;
    public ETypeSize WidthType => _widthType;
    public bool HasChanges { get; private set; }

    public bool SkipLayout
    {
        get
        {
            if (_checkGameObjectEnable && gameObject.activeSelf == false)
            {
                return true;
            }

            return _skipLayout;
        }
    }

    public RectTransform Rect
    {
        get
        {
            if (_rect == null)
            {
                _rect = GetComponent<RectTransform>();
            }

            return _rect;
        }
    }

    private void Update()
    {
        CorrectNaN();
        CheckUpdateParent();
        UpdateSizeByElement();
        CheckSizeChanges();
        UpdateOldDate();
    }

    private void OnDestroy()
    {
        Rect.parent?.GetComponent<DFlexLayout>()?.UpdatedChild();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Update();
    }
#endif

    public void ElemChanged()
    {
        HasChanges = false;
    }

    public (Vector2 size, ETypeSize widthType, ETypeSize heightType) GetSize()
    {
        var size = new Vector2();
        switch (_widthType)
        {
            case ETypeSize.Fixed:
                size.x = _leftMargin + _rightMargin + _width;
                break;
            case ETypeSize.Fill:
                size.x = _width;
                break;
            case ETypeSize.Layout:
            case ETypeSize.Component:
                size.x = _leftMargin + _rightMargin + Rect.rect.width;
                break;
            // case ETypeSize.Layout:
            //     var childs = GetComponentsInChildren<DFlexElement>();
            //     var min = float.MaxValue;
            //     var max = float.MinValue;
            //
            //     if (!childs.Any())
            //     {
            //         size.x = 0;
            //         break;
            //     }
            //     
            //     foreach (var child in childs)
            //     {
            //         if (child.Rect.rect.max.x > max)
            //         {
            //             max = child.Rect.rect.max.x;
            //         }
            //
            //         if (child.Rect.rect.min.x < min)
            //         {
            //             min = child.Rect.rect.min.x;
            //         }
            //     }
            //     size.x = max - min;
            //     size.x += _leftMargin + _rightMargin;
            //     break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (_heightType)
        {
            case ETypeSize.Fixed:
                size.y = _bottomMargin + _topMargin + _height;
                break;
            case ETypeSize.Fill:
                size.y = _height;
                break;
            case ETypeSize.Layout:
            case ETypeSize.Component:
                size.y = _bottomMargin + _topMargin + Rect.rect.height;
                break;
            // case ETypeSize.Layout:
            //     var childs = GetComponentsInChildren<RectTransform>();
            //     var min = float.MaxValue;
            //     var max = float.MinValue;
            //     
            //     if (!childs.Any())
            //     {
            //         size.y = 0;
            //         break;
            //     }
            //     
            //     foreach (var child in childs)
            //     {
            //         if (child.rect.max.y > max)
            //         {
            //             max = child.rect.max.y;
            //         }
            //
            //         if (child.rect.min.y < min)
            //         {
            //             min = child.rect.min.y;
            //         }
            //     }
            //
            //     size.y = max - min;
            //     size.y += _bottomMargin + _topMargin;
            //     break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return (size, _widthType, _heightType);
    }

    public Rect GetRect()
    {
        return Rect.rect;
    }

    public Vector2 GetLocalPos()
    {
        return transform.localPosition;
    }

    public void SetLocalPos(Vector2 pos)
    {
        pos += new Vector2(Rect.rect.size.x * (-0.5f + Rect.pivot.x),Rect.rect.size.y * (-0.5f + Rect.pivot.y));
        transform.localPosition = pos + new Vector2((-_leftMargin + _rightMargin) * Rect.pivot.x, (-_topMargin + _bottomMargin) * Rect.pivot.y);
        _oldLocalPos = Rect.localPosition;
    }

    public void SetSize(float? width, float? height, bool withUpdate = false)
    {
        bool updated = false;
        float tolerance = 0.01f;
        if (_heightType != ETypeSize.Layout && height != null)
        {
            height = (float)height - (_topMargin + _bottomMargin);
        }
        
        if (_widthType != ETypeSize.Layout && width != null)
        {
            width = (float)width - (_leftMargin + _rightMargin);
        }
        
        
        
        if (height != null && Math.Abs((float)height - Rect.rect.size.y) > tolerance)
        {
            if (height < 0)
            {
                height = 0;
            }
            Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (float)height);
            updated = true;
        }

        if (width != null && Math.Abs((float)width - Rect.rect.size.x) > tolerance)
        {
            if (width < 0)
            {
                width = 0;
            }
            Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (float)width);
            updated = true;
        }

        if (updated)
        {
            Debug.Log($"SetSize {gameObject.name} from size = {Rect.rect.size} -- new size = ({width},{height}) , withUpdate = {withUpdate}");
            if (withUpdate == false)
            {
                _oldSize = Rect.rect.size;
            }
        }
        
    }

    private void UpdateSizeByElement()
    {
        var tolerance = 0.001f;

        if (Math.Abs(_width - Rect.sizeDelta.x) > tolerance && _widthType == ETypeSize.Fixed)
        {
            Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _width);
        }

        if (Math.Abs(_height - Rect.sizeDelta.y) > tolerance && _heightType == ETypeSize.Fixed)
        {
            Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _height);
        }
    }

    private void CheckSizeChanges()
    {
        var tolerance = 0.001f;
        if (_oldSkipLayout != _skipLayout ||
            Math.Abs(_oldHeight - _height) > tolerance ||
            _oldHeightType != _heightType ||
            Math.Abs(_oldWidth - _width) > tolerance ||
            _oldWidthType != _widthType ||
            Math.Abs(_oldMinHeight - _minHeight) > tolerance ||
            Math.Abs(_oldMinWidth - _minWidth) > tolerance ||
            Math.Abs(_oldMaxHeight - _maxHeight) > tolerance ||
            Math.Abs(_oldMaxWidth - _maxWidth) > tolerance ||
            Math.Abs(_oldLeftMargin - _leftMargin) > tolerance ||
            Math.Abs(_oldRightMargin - _rightMargin) > tolerance ||
            Math.Abs(_oldTopMargin - _topMargin) > tolerance ||
            Math.Abs(_oldBottomMargin - _bottomMargin) > tolerance ||
            _oldParent != transform.parent //||
            // Math.Abs(_oldLocalPos.x - _rect.localPosition.x) > tolerance ||
            // Math.Abs(_oldLocalPos.y - _rect.localPosition.y) > tolerance
        )
        {
            Debug.Log($"gameobject {gameObject.name} - old parameters changed, elemChangaded");
            HasChanges = true;
        }

        if (Math.Abs(_oldSize.x - Rect.rect.width) > tolerance ||
            Math.Abs(_oldSize.y - Rect.rect.height) > tolerance)
        {
            Debug.Log($"gameobject {gameObject.name} - oldSize = {_oldSize}; newSize = {Rect.rect.size} parameters changed, elemChangaded");
            HasChanges = true;
        }
    }

    private void CheckUpdateParent()
    {
        if (_oldParent != Rect.parent)
        {
            _oldParent?.GetComponent<DFlexLayout>()?.UpdatedChild();
            Rect.parent.GetComponent<DFlexLayout>()?.UpdatedChild();
        }
    }

    private void UpdateOldDate()
    {
        _oldSkipLayout = _skipLayout;
        _oldHeight = _height;
        _oldHeightType = _heightType;
        _oldWidth = _width;
        _oldWidthType = _widthType;
        _oldMinHeight = _minHeight;
        _oldMinWidth = _minWidth;
        _oldMaxHeight = _maxHeight;
        _oldMaxWidth = _maxWidth;
        _oldLeftMargin = _leftMargin;
        _oldRightMargin = _rightMargin;
        _oldTopMargin = _topMargin;
        _oldBottomMargin = _bottomMargin;
        _oldParent = transform.parent;
        _oldSize = Rect.rect.size;
        _oldLocalPos = Rect.localPosition;
    }

    private void CorrectNaN()
    {
        // if (
        //     float.IsNaN(Rect.rect.height) ||
        //     float.IsNaN(Rect.rect.width)
        // || float.IsInfinity(Rect.rect.height) ||
        //     float.IsInfinity(Rect.rect.width)
        // )
        // {
        //     _rect.sizeDelta = Vector2.zero;
        // }
        //
        // if (float.IsNaN(_rect.position.x) || float.IsNaN(_rect.position.y))
        // {
        //     transform.position = new Vector3(0, 0);
        //     HasChanges = true;
        // }
    }
}