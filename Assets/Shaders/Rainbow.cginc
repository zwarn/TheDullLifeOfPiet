float3 toRainbowRGB (float f) {
	f = f - floor(f);
	float a = (1.0 - f) * 6.0;
	int X = a;
	float Y = a - X;
	float3 rgb = float3(0,0,0);			
	switch (X) {
	case 0:
			rgb = float3(1.0,Y,0.0);
			break;
	case 1:
			rgb = float3(1.0-Y,1.0,0.0);
			break;
	case 2:
			rgb = float3(0.0,1.0,Y);
			break;
	case 3:
			rgb = float3(0.0,1.0-Y,1.0);
			break;
	case 4:
			rgb = float3(Y,0.0,1.0);
			break;
	case 5:
			rgb = float3(1.0,0.0,1.0-Y);
			break;
	case 6: 
			rgb = float3(1.0,0.0,0.0);
			break;
	}

	return rgb;
}
