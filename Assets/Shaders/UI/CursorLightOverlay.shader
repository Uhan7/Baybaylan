Shader "UI/Baybaylan Cursor Light"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _DarknessColor ("Darkness Color", Color) = (0, 0, 0, 0.55)
        _LightCenter ("Light Center", Vector) = (0.5, 0.5, 0, 0)
        _RectSize ("Rect Size", Vector) = (1920, 1080, 0, 0)
        _InnerRadius ("Inner Radius", Float) = 10
        _OuterRadius ("Outer Radius", Float) = 2200

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Cursor Light Overlay"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _DarknessColor;
            float2 _LightCenter;
            float2 _RectSize;
            float _InnerRadius;
            float _OuterRadius;

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.texcoord = input.texcoord;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 pixelOffset = (input.texcoord - _LightCenter) * _RectSize;
                float distanceFromLight = length(pixelOffset);
                float darkness = smoothstep(
                    max(0.0, _InnerRadius),
                    max(_InnerRadius + 1.0, _OuterRadius),
                    distanceFromLight);

                fixed4 color = _DarknessColor;
                color.a *= darkness;
                return color;
            }
            ENDCG
        }
    }
}
