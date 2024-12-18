﻿namespace Titan.Platform.Win32.DXGI;

public enum DXGI_INFO_QUEUE_MESSAGE_CATEGORY
{
    DXGI_INFO_QUEUE_MESSAGE_CATEGORY_UNKNOWN = 0,
    DXGI_INFO_QUEUE_MESSAGE_CATEGORY_MISCELLANEOUS = (DXGI_INFO_QUEUE_MESSAGE_CATEGORY_UNKNOWN + 1),
    DXGI_INFO_QUEUE_MESSAGE_CATEGORY_INITIALIZATION = (DXGI_INFO_QUEUE_MESSAGE_CATEGORY_MISCELLANEOUS + 1),
    DXGI_INFO_QUEUE_MESSAGE_CATEGORY_CLEANUP = (DXGI_INFO_QUEUE_MESSAGE_CATEGORY_INITIALIZATION + 1),
    DXGI_INFO_QUEUE_MESSAGE_CATEGORY_COMPILATION = (DXGI_INFO_QUEUE_MESSAGE_CATEGORY_CLEANUP + 1),
    DXGI_INFO_QUEUE_MESSAGE_CATEGORY_STATE_CREATION = (DXGI_INFO_QUEUE_MESSAGE_CATEGORY_COMPILATION + 1),
    DXGI_INFO_QUEUE_MESSAGE_CATEGORY_STATE_SETTING = (DXGI_INFO_QUEUE_MESSAGE_CATEGORY_STATE_CREATION + 1),
    DXGI_INFO_QUEUE_MESSAGE_CATEGORY_STATE_GETTING = (DXGI_INFO_QUEUE_MESSAGE_CATEGORY_STATE_SETTING + 1),
    DXGI_INFO_QUEUE_MESSAGE_CATEGORY_RESOURCE_MANIPULATION = (DXGI_INFO_QUEUE_MESSAGE_CATEGORY_STATE_GETTING + 1),
    DXGI_INFO_QUEUE_MESSAGE_CATEGORY_EXECUTION = (DXGI_INFO_QUEUE_MESSAGE_CATEGORY_RESOURCE_MANIPULATION + 1),
    DXGI_INFO_QUEUE_MESSAGE_CATEGORY_SHADER = (DXGI_INFO_QUEUE_MESSAGE_CATEGORY_EXECUTION + 1)
}
