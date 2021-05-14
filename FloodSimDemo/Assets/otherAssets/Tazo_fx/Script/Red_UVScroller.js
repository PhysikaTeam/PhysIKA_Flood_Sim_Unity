
var targetMaterialSlot:int=0;
//var scrollThis:Material;
var speedY:float=0.5;
var speedX:float=0.0;
private var timeWentX:float=0;
private var timeWentY:float=0;

function Start () {
rend=GetComponent.<Renderer>();
//rend.enable=true;
}

function Update () {
timeWentY += Time.deltaTime*speedY;
timeWentX += Time.deltaTime*speedX;

rend=GetComponent.<Renderer>();

rend.materials[targetMaterialSlot].SetTextureOffset ("_MainTex", Vector2(timeWentX, timeWentY));


}

function OnEnable(){
rend=GetComponent.<Renderer>();

	rend.materials[targetMaterialSlot].SetTextureOffset ("_MainTex", Vector2(0, 0));
	timeWentX = 0;
	timeWentY = 0;
}