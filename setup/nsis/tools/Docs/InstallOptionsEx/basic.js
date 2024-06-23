<!-- Expand/Collapse Effect -->

/***********************************************
* Switch Content script- © Dynamic Drive (www.dynamicdrive.com)
* This notice must stay intact for legal use. Last updated Mar 23rd, 2004.
* Visit http://www.dynamicdrive.com/ for full source code
***********************************************/

var pszAnchorPrefix = "a_";
var pszDivisionPrefix = "sc";

function Expand(cid){
  document.getElementById(cid).style.display=(document.getElementById(cid).style.display!="block")? "block" : "none"
}

function ExpandByName(name){
  Expand(getElementbyAnchorName(name));
}

function getElementbyAnchorName(name){
  var tags=document.getElementsByTagName("div");
  for (i=0; i<tags.length; i++)
    if(tags[i].name.substr(0,pszAnchorPrefix.length) == pszAnchorPrefix && tags[i].name.substr(pszAnchorPrefix.length) == name)
      return pszDivisionPrefix+tags[i].id;
  return -1;
}

function ExpandAll(){
  var i=0;
  var ia=0;
  var tags=document.getElementsByTagName("div");
  for (i=0; i<tags.length; i++){
    if(tags[i].id.substr(0,pszDivisionPrefix.length) == pszDivisionPrefix)
    {
      ia++;
      document.getElementById(pszDivisionPrefix+ia).style.display="block"
    }
  }
}

function CollapseAll(){
  var i=0;
  var ia=0;
  var tags=document.getElementsByTagName("div");
  for (i=0; i<tags.length; i++){
    if(tags[i].id.substr(0,pszDivisionPrefix.length) == pszDivisionPrefix)
    {
      ia++;
        document.getElementById(pszDivisionPrefix+ia).style.display="none"
    }
  }
}

function InverseAll(){
  var i=0;
  var ia=0;
  var tags=document.getElementsByTagName("div");
  for (i=0; i<tags.length; i++){
    if(tags[i].id.substr(0,pszDivisionPrefix.length) == pszDivisionPrefix)
    {
      ia++;
      document.getElementById(pszDivisionPrefix+ia).style.display=(document.getElementById(pszDivisionPrefix+ia).style.display!="block")? "block" : "none"
    }
  }
}

var nTable = 0;

<!-- Write -->

var nLevel = -1;
var nLevelItem = -1;

var pszLevels = "";
var aLevels = [0];

function WriteTitleStart(style, name) {
  nTable++;

  nLevel++;
  if(aLevels[nLevel] == undefined)
    aLevels[nLevel] = 1;
  else
    aLevels[nLevel]++;
  var nLevelTemp = aLevels.length - 1;
  while(nLevel < nLevelTemp--)
  {
    delete aLevels[nLevelTemp+1];
    aLevels.length--;
  }
  pszLevels = aLevels.join(".");

  document.write('<table cellspacing=0 ');

  if(style == "" || style == undefined)
  {
    if(nLevel == 0)
      document.write('class=ti');
    else
      document.write('class=tia');
  }
  else if(style == "IOEx")
    document.write('class=tian');
  else if(style == "IO")
    document.write('class=tiao');

  if(name == undefined)
    name = "";

  //+pszAnchorPrefix+name+
  document.write('><tr><td>&nbsp;<a id="'+nTable+'" name="'+pszLevels+'" onClick="Expand(' + "'" + pszDivisionPrefix + nTable + "'" + ')"><b><font color="#DDDDDD">'+pszLevels+'</font> ');
}
function WriteTitleEnd() {
  nLevel--;
  document.write('</b></a></td></tr></table>');
}

function WriteTextStart(style) {
  nLevel++;
  document.write('<div id="' + pszDivisionPrefix + nTable + '" class=sc><table ');

  if(style == "" || style == undefined)
    if(nLevel == 0)
      document.write('class=tx');
    else
      document.write('class=txa');
  else if(style == "IOEx")
    document.write('class=txan');
  else if(style == "IO")
    document.write('class=txao');

  document.write('><tr><td>');
}

function WriteTextEnd() {
  nLevel--;
  document.write('</td></tr></table></div>');
}
