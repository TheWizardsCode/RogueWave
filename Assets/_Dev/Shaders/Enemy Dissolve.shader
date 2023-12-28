// Made with Amplify Shader Editor v1.9.2.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Enemy Dissolve"
{
	Properties
	{
		_DiffuseColor("Diffuse Color", Color) = (0.09011214,0.3107221,0.7075472,1)
		[PerRendererData]_Dissolve("Dissolve", Range( 0 , 1)) = 0
		_Cutoff( "Mask Clip Value", Float ) = 0.15
		_DissolveMask("DissolveMask", 2D) = "white" {}
		_GridScale("Grid Scale", Range( 1 , 100)) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
		};

		uniform float4 _DiffuseColor;
		uniform sampler2D _DissolveMask;
		uniform float _GridScale;
		uniform float _Dissolve;
		uniform float _Cutoff = 0.15;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _DiffuseColor.rgb;
			o.Alpha = 1;
			float3 ase_worldPos = i.worldPos;
			float3 break33 = ( ase_worldPos * _GridScale );
			float3 appendResult36 = (float3(round( break33.x ) , round( break33.y ) , round( break33.z )));
			float3 temp_output_41_0 = ( appendResult36 / _GridScale );
			float lerpResult46 = lerp( 0.25 , 8.0 , _Dissolve);
			clip( pow( ( tex2D( _DissolveMask, (temp_output_41_0).xy ).r + tex2D( _DissolveMask, (temp_output_41_0).yz ).r + tex2D( _DissolveMask, (temp_output_41_0).xz ).r ) , lerpResult46 ) - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	/*CustomEditor "ASEMaterialInspector"*/
}
/*ASEBEGIN
Version=19202
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Enemy Dissolve;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Masked;0.15;True;True;0;False;TransparentCutout;;AlphaTest;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;2;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.BreakToComponentsNode;33;-3446.141,514.0164;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;-3754.25,512.7157;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldToObjectMatrix;12;-4348.015,613.949;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-4092.249,513.4988;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;36;-3061.347,514.9164;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RoundOpNode;32;-3255.451,472.0163;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;35;-3256.751,553.9169;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;34;-3256.751,637.1166;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-4028.95,870.5138;Inherit;False;Property;_GridScale;Grid Scale;4;0;Create;True;0;0;0;False;0;False;0;100;1;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;41;-2791.149,723.2185;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;16;-2214.492,564.2471;Inherit;False;FLOAT2;1;2;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;17;-2213.492,691.2474;Inherit;False;FLOAT2;0;2;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;15;-2213.177,441.3853;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;10;-2320.786,202.9849;Inherit;True;Property;_DissolveMask;DissolveMask;3;0;Create;True;0;0;0;False;0;False;None;50ca3e8355f126a46923ef1732b7529e;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;18;-1778.779,428.7943;Inherit;True;Property;_TextureSample1;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;19;-1775.561,661.6063;Inherit;True;Property;_TextureSample2;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;42;-952.8184,236.1319;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;46;-672.8569,578.7228;Inherit;False;3;0;FLOAT;0.25;False;1;FLOAT;8;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;9;-1781.179,207.814;Inherit;True;Property;_TextureSample0;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;45;-571.6066,249.3306;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-547.2401,-26.82982;Inherit;False;Property;_DiffuseColor;Diffuse Color;0;0;Create;True;0;0;0;False;0;False;0.09011214,0.3107221,0.7075472,1;0.02202741,0.9339623,0.08684548,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;7;-1074.747,651.5781;Inherit;False;Property;_Dissolve;Dissolve;1;1;[PerRendererData];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;11;-4158.644,307.6574;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
WireConnection;0;0;1;0
WireConnection;0;10;45;0
WireConnection;33;0;37;0
WireConnection;37;0;11;0
WireConnection;37;1;38;0
WireConnection;14;0;11;0
WireConnection;14;1;12;0
WireConnection;36;0;32;0
WireConnection;36;1;35;0
WireConnection;36;2;34;0
WireConnection;32;0;33;0
WireConnection;35;0;33;1
WireConnection;34;0;33;2
WireConnection;41;0;36;0
WireConnection;41;1;38;0
WireConnection;16;0;41;0
WireConnection;17;0;41;0
WireConnection;15;0;41;0
WireConnection;18;0;10;0
WireConnection;18;1;16;0
WireConnection;19;0;10;0
WireConnection;19;1;17;0
WireConnection;42;0;9;1
WireConnection;42;1;18;1
WireConnection;42;2;19;1
WireConnection;46;2;7;0
WireConnection;9;0;10;0
WireConnection;9;1;15;0
WireConnection;45;0;42;0
WireConnection;45;1;46;0
ASEEND*/
//CHKSM=7D47BBA63BB2ACD6CD3CC653F84F5F9D19270567