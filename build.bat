::����ģ������
SET ToolName=ybwork.GPUSkinning
::����ģ��汾
SET ToolVersion=upm
::����ģ��Դ·��
SET ToolAssetPath=Assets/ybwork/GPUSkinning

git branch -D %ToolName%
git remote rm %ToolName%
::������ᴴ��һ��ToolName�ķ�֧����ͬ��ToolAssetPath�µ�����
git subtree split -P %ToolAssetPath% --branch %ToolName%
:: ��ToolName��֧���ñ�ǩToolVersion�ڵ�
git tag -d %ToolVersion%
git tag %ToolVersion% %ToolName%

:: ���͵�Զ��
git push origin -f %ToolName% %ToolVersion%
git push origin %ToolName%
pause
