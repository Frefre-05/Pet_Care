from enum import IntFlag

import comtypes.gen._00020430_0000_0000_C000_000000000046_0_2_0 as __wrapper_module__
from comtypes.gen._00020430_0000_0000_C000_000000000046_0_2_0 import (
    StdPicture, Picture, CoClass, VARIANT_BOOL, Monochrome,
    FONTSTRIKETHROUGH, _check_version, OLE_XSIZE_CONTAINER, OLE_COLOR,
    IPicture, StdFont, FONTSIZE, IPictureDisp, DISPPARAMS,
    OLE_YSIZE_HIMETRIC, FONTBOLD, OLE_YSIZE_PIXELS, _lcid,
    OLE_YSIZE_CONTAINER, OLE_ENABLEDEFAULTBOOL, OLE_HANDLE,
    IEnumVARIANT, Default, OLE_XSIZE_PIXELS, FONTNAME, COMMETHOD,
    OLE_CANCELBOOL, OLE_XPOS_HIMETRIC, OLE_YPOS_HIMETRIC, Gray,
    IFontEventsDisp, VgaColor, OLE_XPOS_PIXELS, BSTR, dispid,
    Unchecked, FONTITALIC, Library, OLE_YPOS_CONTAINER, IFontDisp,
    typelib_path, HRESULT, IFont, Font, DISPMETHOD,
    OLE_XPOS_CONTAINER, OLE_OPTEXCLUSIVE, GUID, Color, IUnknown,
    OLE_YPOS_PIXELS, Checked, FONTUNDERSCORE, EXCEPINFO, IDispatch,
    FontEvents, OLE_XSIZE_HIMETRIC, DISPPROPERTY
)


class LoadPictureConstants(IntFlag):
    Default = 0
    Monochrome = 1
    VgaColor = 2
    Color = 4


class OLE_TRISTATE(IntFlag):
    Unchecked = 0
    Checked = 1
    Gray = 2


__all__ = [
    'StdPicture', 'Picture', 'FONTNAME', 'OLE_CANCELBOOL',
    'OLE_XPOS_HIMETRIC', 'OLE_YPOS_HIMETRIC', 'Monochrome',
    'LoadPictureConstants', 'Gray', 'FONTSTRIKETHROUGH',
    'IFontEventsDisp', 'VgaColor', 'OLE_XPOS_PIXELS',
    'OLE_XSIZE_CONTAINER', 'Unchecked', 'OLE_COLOR', 'FONTITALIC',
    'Library', 'IPicture', 'OLE_YPOS_CONTAINER', 'StdFont',
    'IFontDisp', 'FONTSIZE', 'IPictureDisp', 'typelib_path', 'IFont',
    'Font', 'OLE_XSIZE_HIMETRIC', 'OLE_YSIZE_HIMETRIC', 'FONTBOLD',
    'OLE_XPOS_CONTAINER', 'OLE_OPTEXCLUSIVE', 'OLE_YSIZE_PIXELS',
    'Color', 'OLE_YPOS_PIXELS', 'Checked', 'FONTUNDERSCORE',
    'OLE_YSIZE_CONTAINER', 'OLE_ENABLEDEFAULTBOOL', 'OLE_TRISTATE',
    'OLE_HANDLE', 'FontEvents', 'Default', 'OLE_XSIZE_PIXELS'
]

