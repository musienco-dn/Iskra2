// VacuumShaders 2015
// https://www.facebook.com/VacuumShaders

Shader "VacuumShaders/Terrain To Mesh/One Directional Light/Diffuse/2 Textures" 
{
	Properties     
	{
		_Color("Color", color) = (1, 1, 1, 1)
		[NoScaleOffset]_V_T2M_Control ("Control Map (RGBA)", 2D) = "black" {}

		//TTM				
		[V_T2M_Layer] _V_T2M_Splat1 ("Layer 1 (R)", 2D) = "white" {}
		[HideInInspector] _V_T2M_Splat1_uvScale("", float) = 1	

		[V_T2M_Layer] _V_T2M_Splat2 ("Layer 2 (G)", 2D) = "white" {}
		[HideInInspector] _V_T2M_Splat2_uvScale("", float) = 1	

		_V_T2M_Lightmap_Multiplier("Lightmap Multiplier", float) = 1


		//Fallback use only
		[NoScaleOffset]_MainTex ("BaseMap (Fallback use only!)", 2D) = "white" {}
	}
	 
	 
	SubShader   
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		     
		Pass 
	    {   
			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" } 

			CGPROGRAM
			#pragma vertex vert 
	    	#pragma fragment frag  		  
			#pragma multi_compile_fwdbase nodynlightmap nodirlightmap
			#pragma multi_compile_fog
			#pragma target 2.0
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"


			#include "../cginc/T2M_ODL.cginc" 

			ENDCG

		} //Pass

	} //SubShader
	 
	FallBack "Hidden/VacuumShaders/Fallback/VertexLit"

} //Shader
