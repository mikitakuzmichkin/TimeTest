using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(DFlexElement), typeof(RectTransform))]
public partial class DFlexLayout : MonoBehaviour
{
    [SerializeField] private ETypeDirection _direction;
    
    [Header("Wrap")]
    [SerializeField] private bool _wrap;
    [SerializeField] private ETypeGap _wrapGapType;
    [SerializeField] private float _wrapGap;
    
    [Header("Align")]
    [SerializeField] private ETypeAlign _horizontalAlign;
    [SerializeField] private ETypeAlign _verticalAlign;
    [SerializeField] private ETypeAlign _innerHorizontalAlign;
    [SerializeField] private ETypeAlign _innerVerticalAlign;
    
    [Header("Gap")]
    [SerializeField] private ETypeGap _gapType;
    [SerializeField] private float _gap;
    
    [Header("Padding")] 
     [SerializeField] private float _leftPadding;
     [SerializeField] private float _rightPadding;
     [SerializeField] private float _topPadding;
     [SerializeField] private float _bottomPadding;

    private DFlexElement _flexElement;
    private RectTransform _rect;
    private List<DFlexElement> _flexChilds;
    private bool _needUpdateChilds;
    
    private ETypeDirection _oldDirection;
    private ETypeAlign _oldHorizontalAlign;
    private ETypeAlign _oldVerticalAlign;
    private ETypeAlign _oldInnerHorizontalAlign;
    private ETypeAlign _oldInnerVerticalAlign;
    private ETypeGap _oldGapType;
    private float _oldGap;

    private bool _oldWrap;
    private ETypeGap _oldWrapGapType;
    private float _oldWrapGap;
    
    private float _oldLeftPadding;
    private float _oldRightPadding;
    private float _oldTopPadding;
    private float _oldBottomPadding;

    private Vector2 _oldSize;
    private Vector3 _oldPos;

    private DFlexElement _FlexElement
    {
        get
        {
            if (_flexElement == null)
            {
                _flexElement = GetComponent<DFlexElement>();
            }

            return _flexElement;
        }
    }

    private RectTransform _Rect
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

    private void LateUpdate()
    {
        bool wasUpdateChilds = false;
        if (_flexChilds == null || _needUpdateChilds || CheckChilds())
        {
            UpdateChilds();
            wasUpdateChilds = true;
        } 
        if (CheckUpdate() || CheckLayoutChanges() || wasUpdateChilds)
        {
            Debug.Log(gameObject.name + " is reloaded");
            UpdateElements();
        }

        UpdateOldChanges();
    }

    private void UpdateChilds()
    {
        _flexChilds = GetComponentsInChildren<DFlexElement>(false).Where(c => c.transform.parent == transform).ToList();
        _flexChilds.Remove(_FlexElement);
        _needUpdateChilds = false;
    }

    private bool CheckChilds()
    {
        foreach (var child in _flexChilds)
        {
            if (child == null || child.gameObject == null)
            {
                return true;
            }
        }

        return false;
    }

    private void OnTransformChildrenChanged()
    {
        Debug.Log("OnTransformChildrenChanged");
        _needUpdateChilds = true;
    }

    public void UpdatedChild()
    {
        _needUpdateChilds = true;
    }

    private bool CheckUpdate()
    {
        foreach (var child in _flexChilds)
        {
            if (child.HasChanges)
            {
                Debug.Log($"gameobject {gameObject.name} has changes");
                return true;
            }
        }

        // if (_FlexElement.HasChanges)
        // {
        //     Debug.Log($"gameobject {gameObject.name} layout has changes");
        //     return true;
        // }

        return false;
    }

    private void UpdateElements()
    {
        if (_direction == ETypeDirection.PositiveX && _wrap && _FlexElement.GetSize().widthType != DFlexElement.ETypeSize.Layout)
        {
            SetChildWrap();
        }
        else
        {
            var sizeParams = GetChildSizeParams(_flexChilds.Where(c => c.SkipLayout == false), _direction, _Rect.rect, _gapType, _gap, 
                _topPadding, _bottomPadding, _leftPadding, _rightPadding);
            var fillSize = GetFillSize(sizeParams.sumSize, sizeParams.fillCount, _Rect.rect);
            SetChildPositions(sizeParams.sumSize, new Vector2(sizeParams.maxWidth, sizeParams.maxHeight), fillSize, sizeParams.fillCount);
        }
        
        foreach (var child in _flexChilds)
        {
            child.ElemChanged();
        }
    }

    private Vector2 GetFillSize(Vector2 sumSize, Vector2 fillCount, Rect rect)
    {
        var x = Mathf.Max(0, (rect.width - sumSize.x - _leftPadding - _rightPadding) / (fillCount.x * 1f));
        var y = Mathf.Max(0, (rect.height - sumSize.y - _topPadding - _bottomPadding) / (fillCount.y * 1f));
        return new Vector2(x, y);
    }

    private void SetChildPositions(Vector2 sumSize, Vector2 maxSize, Vector2 fillSize, Vector2Int fillCount)
    {
        float? width = null;
        float? height = null;
        switch (_direction)
        {
            case ETypeDirection.PositiveX:
                if (_FlexElement.HeightType == DFlexElement.ETypeSize.Layout ||
                    _FlexElement.WidthType == DFlexElement.ETypeSize.Layout)
                {
                    if (_FlexElement.WidthType == DFlexElement.ETypeSize.Layout)
                    {
                        width = sumSize.x;
                    }

                    if (_FlexElement.HeightType == DFlexElement.ETypeSize.Layout)
                    {
                        height = maxSize.y
                                 + _topPadding + _bottomPadding;
                    }
                    _FlexElement.SetSize(width, height, true);
                }
                break;
            case ETypeDirection.PositiveY:
                if (_FlexElement.HeightType == DFlexElement.ETypeSize.Layout ||
                    _FlexElement.WidthType == DFlexElement.ETypeSize.Layout)
                {
                    if (_FlexElement.WidthType == DFlexElement.ETypeSize.Layout)
                    {
                        width = maxSize.x
                                + _leftPadding + _rightPadding;
                    }

                    if (_FlexElement.HeightType == DFlexElement.ETypeSize.Layout)
                    {
                        height = sumSize.y;
                    }
                    _FlexElement.SetSize(width, height
                       , true);
                }
                break;
            // case ETypeDirection.NegativeX:
            //     break;
            // case ETypeDirection.NegativeY:
            //     break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var parentSize = _Rect.rect.size;
        if (_FlexElement.WidthType == DFlexElement.ETypeSize.Layout && _direction == ETypeDirection.PositiveY)
        {
            parentSize.x = maxSize.x;
        }
        
        if (_FlexElement.HeightType == DFlexElement.ETypeSize.Layout && _direction == ETypeDirection.PositiveX)
        {
            parentSize.y = maxSize.y;
        }

        var childs = _flexChilds.Where(c => c.SkipLayout == false);
        {
            SetChildPositionsPositive(_direction, sumSize, maxSize, fillSize, fillCount, childs, 
                parentSize, _gap, _gapType, _Rect.pivot);
        }
        
    }
    
    private bool CheckLayoutChanges()
    {
        float TOLERANCE = 0.01f;
        if (_oldDirection != _direction)
        {
            Debug.Log($"gameobject {gameObject.name} direction changed");
            return true;
        }
        
        if (_oldHorizontalAlign != _horizontalAlign ||
            _oldVerticalAlign != _verticalAlign ||
            _oldInnerHorizontalAlign != _innerHorizontalAlign ||
            _oldInnerVerticalAlign != _innerVerticalAlign)
        {
            Debug.Log($"gameobject {gameObject.name} align changed");
            return true;
        }
        
        if (_oldGapType != _gapType ||
            Math.Abs(_oldGap - _gap) > TOLERANCE)
        {
            Debug.Log($"gameobject {gameObject.name} gap changed");
            return true;
        }
        
        if (Math.Abs(_oldLeftPadding - _leftPadding) > TOLERANCE ||
            Math.Abs(_oldRightPadding - _rightPadding) > TOLERANCE ||
            Math.Abs(_oldTopPadding - _topPadding) > TOLERANCE ||
            Math.Abs(_oldBottomPadding - _bottomPadding) > TOLERANCE)
        {
            Debug.Log($"gameobject {gameObject.name} paddings changed");
            return true;
        } 
        
        if (_wrap != _oldWrap
            || _wrapGapType != _oldWrapGapType
            || Math.Abs(_wrapGap - _oldWrapGap) > TOLERANCE)
        {
            Debug.Log($"gameobject {gameObject.name} wrap changed");
            return true;
        }

        // if (Math.Abs(_oldPos.x - _Rect.position.x) > TOLERANCE || 
        //     Math.Abs(_oldPos.y - _Rect.position.y) > TOLERANCE)
        // {
        //     Debug.Log($"gameobject {gameObject.name} pos changed");
        //     return true;
        // }
        
        if (Math.Abs(_oldSize.x - _Rect.rect.size.x) > TOLERANCE || 
            Math.Abs(_oldSize.y - _Rect.rect.size.y) > TOLERANCE)
        {
            Debug.Log($"gameobject {gameObject.name} size changed");
            return true;
        }

        return false;
    }

    private void UpdateOldChanges()
    {
        _oldDirection = _direction;
            _oldHorizontalAlign = _horizontalAlign;
            _oldVerticalAlign = _verticalAlign;
            _oldInnerHorizontalAlign = _innerHorizontalAlign;
            _oldInnerVerticalAlign = _innerVerticalAlign;
            _oldGapType = _gapType;
            _oldGap = _gap;
            _oldLeftPadding = _leftPadding;
            _oldRightPadding = _rightPadding;
            _oldTopPadding = _topPadding;
            _oldBottomPadding = _bottomPadding;

            _oldWrap = _wrap;
            _oldWrapGap = _wrapGap;
            _oldWrapGapType = _wrapGapType;
            _oldSize = _Rect.rect.size;
            _oldPos = _Rect.position;
    }
    
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        LateUpdate();
    }
#endif

    public enum ETypeDirection
    {
        PositiveX,
        PositiveY,
        // NegativeX,
        // NegativeY
    }
    
    public enum ETypeAlign
    {
        None,
        Start,
        Center,
        End
    }

    public enum ETypeGap
    {
        Fixed,
        SpaceBetween
    }
}
