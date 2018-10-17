#ifndef CTMACROS_HPP
#define CTMACROS_HPP

#ifdef _WIN32
#ifndef ARIASDK_SPEC // we use __cdecl by default
#define ARIASDK_SPEC __cdecl
#define ARIASDK_LIBABI_CDECL __cdecl
#  if defined(ARIASDK_SHARED_LIB)
#    define ARIASDK_LIBABI __declspec(dllexport)
#  elif defined(ARIASDK_STATIC_LIB)
#    define ARIASDK_LIBABI
#  else // Header file included by client
#    ifndef ARIASDK_LIBABI
#    define ARIASDK_LIBABI
#    endif
#  endif
#endif

#else // non-windows platform

#ifndef ARIASDK_SPEC 
#define ARIASDK_SPEC
#endif

#ifndef ARIASDK_LIBABI_CDECL
#define ARIASDK_LIBABI_CDECL
#endif

#ifndef ARIASDK_LIBABI 
#define ARIASDK_LIBABI
#endif
#endif
#endif
