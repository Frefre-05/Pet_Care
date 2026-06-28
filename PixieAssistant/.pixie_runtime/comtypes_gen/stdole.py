from enum import IntFlag

import comtypes.gen._00020430_0000_0000_C000_000000000046_0_2_0 as __wrapper_module__
from comtypes.gen._00020430_0000_0000_C000_000000000046_0_2_0 import (
    StdFont, OLE_YSIZE_CONTAINER, _lcid, OLE_OPTEXCLUSIVE, OLE_HANDLE,
    IUnknown, OLE_XSIZE_HIMETRIC, FONTUNDERSCORE, Unchecked,
    EXCEPINFO, Default, OLE_CANCELBOOL, typelib_path,
    OLE_XPOS_CONTAINER, FONTSIZE, OLE_XPOS_PIXELS, VgaColor,
    OLE_ENABLEDEFAULTBOOL, Gray, IDispatch, _check_version, Font,
    OLE_YSIZE_HIMETRIC, FONTSTRIKETHROUGH, IEnumVARIANT,
    OLE_XPOS_HIMETRIC, Checked, OLE_XSIZE_CONTAINER, OLE_YPOS_PIXELS,
    CoClass, OLE_YPOS_HIMETRIC, IPictureDisp, Color, HRESULT,
    DISPPROPERTY, DISPPARAMS, BSTR, DISPMETHOD, FONTNAME, Picture,
    FONTBOLD, FONTITALIC, IPicture, GUID, VARIANT_BOOL,
    OLE_YPOS_CONTAINER, IFontEventsDisp, Library, StdPicture,
    OLE_YSIZE_PIXELS, OLE_XSIZE_PIXELS, FontEvents, COMMETHOD,
    IFontDisp, dispid, OLE_COLOR, Monochrome, IFont
)


class OLE_TRISTATE(IntFlag):
    Unchecked = 0
    Checked = 1
    Gray = 2


class LoadPictureConstants(IntFlag):
    Default = 0
    Monochrome = 1
    VgaColor = 2
    Color = 4


__all__ = [
    'LoadPictureConstants', 'Monochrome', 'OLE_YSIZE_CONTAINER',
    'OLE_OPTEXCLUSIVE', 'OLE_HANDLE', 'OLE_XSIZE_HIMETRIC',
    'FONTUNDERSCORE', 'Unchecked', 'FONTNAME', 'Picture', 'FONTBOLD',
    'FONTITALIC', 'Default', 'OLE_CANCELBOOL', 'IPicture',
    'typelib_path', 'OLE_XPOS_CONTAINER', 'FONTSIZE',
    'OLE_XPOS_PIXELS', 'VgaColor', 'OLE_ENABLEDEFAULTBOOL', 'Gray',
    'OLE_YPOS_CONTAINER', 'Font', 'OLE_YSIZE_HIMETRIC',
    'FONTSTRIKETHROUGH', 'IFontEventsDisp', 'Library', 'StdPicture',
    'OLE_YSIZE_PIXELS', 'Checked', 'OLE_XPOS_HIMETRIC',
    'OLE_XSIZE_PIXELS', 'OLE_XSIZE_CONTAINER', 'Color', 'FontEvents',
    'OLE_YPOS_PIXELS', 'IFontDisp', 'OLE_YPOS_HIMETRIC',
    'IPictureDisp', 'StdFont', 'OLE_COLOR', 'IFont', 'OLE_TRISTATE'
]

