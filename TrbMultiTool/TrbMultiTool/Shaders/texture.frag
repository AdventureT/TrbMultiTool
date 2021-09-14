//Fragment shader
#version 110
uniform sampler2D ColorTable;     //256 x 1 pixels
uniform sampler2D MyIndexTexture;
varying vec2 TexCoord;

void main()
{
  //What color do we want to index?
  vec4 myindex = texture2D(MyIndexTexture, TexCoord);
  //Do a dependency texture read
  vec4 texel = texture2D(ColorTable, myindex.xy);
  gl_FragColor = texel;   //Output the color
}