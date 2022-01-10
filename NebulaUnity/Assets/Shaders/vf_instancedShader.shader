   Shader "Custom/VF_InstancedShader" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader {

        Pass {

            Tags {"LightMode"="ForwardBase"}

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma target 5.0

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "AutoLight.cginc"

            sampler2D _MainTex;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv_MainTex : TEXCOORD0;
                float3 ambient : TEXCOORD1;
                float3 diffuse : TEXCOORD2;
                float3 color : TEXCOORD3;
                SHADOW_COORDS(4)
            };

            struct GPUObject {
                uint objID;
                float x;
                float y;
                float z;
                float rotx;
                float roty;
                float rotz;
                float rotw;
            };

            struct appdata_my{
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                float4 texcoord2 : TEXCOORD2;
                float4 texcoord3 : TEXCOORD3;
                fixed4 color : COLOR;

                uint id : SV_VertexID;
            };

            struct anim_data{
                float time;
                float prep_len;
                float work_len;
                uint state;
                float power;
            };

        #if SHADER_TARGET >= 45
            StructuredBuffer<GPUObject> _InstBuffer;
            StructuredBuffer<uint> _IdBuffer;

            StructuredBuffer<float> _VertaBuffer;
            StructuredBuffer<anim_data> _AnimBuffer;

            int _VertexSize;
            int _VertexCount;
            int _FrameCount;

        #endif


            float4x4 quaternionToMatrix(float4 quat)
            {
                float4x4 m = float4x4(float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0));

                float x = quat.x, y = quat.y, z = quat.z, w = quat.w;
                float x2 = x + x, y2 = y + y, z2 = z + z;
                float xx = x * x2, xy = x * y2, xz = x * z2;
                float yy = y * y2, yz = y * z2, zz = z * z2;
                float wx = w * x2, wy = w * y2, wz = w * z2;

                m[0][0] = 1.0 - (yy + zz);
                m[0][1] = xy - wz;
                m[0][2] = xz + wy;

                m[1][0] = xy + wz;
                m[1][1] = 1.0 - (xx + zz);
                m[1][2] = yz - wx;

                m[2][0] = xz - wy;
                m[2][1] = yz + wx;
                m[2][2] = 1.0 - (xx + yy);

                m[3][3] = 1.0;

                return m;
            }

            v2f vert (appdata_my v, uint instanceID : SV_InstanceID)
            {
                v2f o;  
                bool batched = false;

            #if SHADER_TARGET >= 45
                GPUObject data = _InstBuffer[instanceID];
                if (data.objID != 0) {
                    anim_data anim = _AnimBuffer[data.objID];
                    uint frame = round(anim.time / anim.work_len * _FrameCount);
                    int vertStart = v.id * _VertexSize;
                    int globalVertStart = _VertexSize * _VertexCount * frame + vertStart;

                    if (_VertexSize == 3){
                        float3 pos = float3(_VertaBuffer[globalVertStart], _VertaBuffer[globalVertStart + 1], _VertaBuffer[globalVertStart + 2]);
                        v.vertex = float4(pos, 0);
                    }else if (_VertexSize == 6) {
                        float3 pos = float3(_VertaBuffer[globalVertStart], _VertaBuffer[globalVertStart + 1], _VertaBuffer[globalVertStart + 2]);
                        float3 normal = float3(_VertaBuffer[globalVertStart + 3], _VertaBuffer[globalVertStart + 4], _VertaBuffer[globalVertStart + 5]);
                        v.vertex = float4(pos, 0);
                        v.normal = normal;
                    }else if (_VertexSize == 9) {
                        float3 pos = float3(_VertaBuffer[globalVertStart], _VertaBuffer[globalVertStart + 1], _VertaBuffer[globalVertStart + 2]);
                        float3 normal = float3(_VertaBuffer[globalVertStart + 3], _VertaBuffer[globalVertStart + 4], _VertaBuffer[globalVertStart + 5]);
                        float3 tangent = float3(_VertaBuffer[globalVertStart + 6], _VertaBuffer[globalVertStart + 7], _VertaBuffer[globalVertStart + 8]);
                        v.vertex = float4(pos, 0);
                        v.normal = normal;
                        v.tangent = float4(tangent, 0);
                    }

                    float4x4 rotMat = quaternionToMatrix(float4(data.rotx, data.roty, data.rotz, data.rotw));
                    //float rotation = data.w * data.w * _Time.x * 0.5f;
                    //rotate2D(data.xz, rotation);

                    float3 worldPosition = mul(rotMat, v.vertex.xyz);
                    worldPosition += float3(data.x, data.y, data.z);
                    o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));

                    batched = true;
                }
            #endif

                if (!batched) {

                    float4x4 rotMat = quaternionToMatrix(float4(data.rotx, data.roty, data.rotz, data.rotw));
                    //float rotation = data.w * data.w * _Time.x * 0.5f;
                    //rotate2D(data.xz, rotation);

                    float3 worldPosition = mul(rotMat, v.vertex.xyz);
                    worldPosition += float3(data.x, data.y, data.z);
                    o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));

                }

                float3 ndotl = saturate(dot(v.normal, _WorldSpaceLightPos0.xyz));
                float3 ambient = ShadeSH9(float4(v.normal, 1.0f));
                float3 diffuse = (ndotl * _LightColor0.rgb);
                float3 color = v.color;

                o.uv_MainTex = v.texcoord;
                o.ambient = ambient;
                o.diffuse = diffuse;
                o.color = color;
                TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed shadow = SHADOW_ATTENUATION(i);
                fixed4 albedo = tex2D(_MainTex, i.uv_MainTex);
                float3 lighting = i.diffuse * shadow + i.ambient;
                fixed4 output = fixed4(albedo.rgb * i.color * lighting, albedo.w);
                UNITY_APPLY_FOG(i.fogCoord, output);
                return output;
            }

            ENDCG
        }
    }
}