// AssetPatcher.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include "pch.h"
#define _CRT_SECURE_NO_WARNINGS
#include "AssetsFileFormat.h"
#include "AssetsFileReader.h"
#include "AssetTypeClass.h"
#include "AssetsFileTable.h"
#include <windows.h>
#include <iostream>
#include <fstream>
#include <string>

using std::string;
using std::cout;



std::ifstream::pos_type filesize(const char* filename)
{
	std::ifstream in(filename, std::ifstream::ate | std::ifstream::binary);
	return in.tellg();
}

int main(int argc, char *argv[])
{
	cout << argc;
	if (argc < 5) {
		cout << "AssetPatcher <assetFile> <path with files to put in> <file type> <unity class id>\r\n";
		cout << "files to put in should be in 12345__789_filename.assset format where 12345 is the pathid you want it to have and 789 is a pathid of a resource to copy the types/classes from.\r\n";
		return -1;
	}
	string assetFile = string(argv[1]);
	string path = string(argv[2]);
	string pattern = string(argv[2]);
	unsigned int fileType = std::stoul(argv[3]);
	int unityClass = std::stoi(argv[4]);
	pattern.append("\\*_*.asset");
	cout << "Using file type: " << fileType << ", unity class: " << unityClass << "\r\nFinding files in " << path << "\r\n";


	//cout << "Opening asset file: " << assetFile << "\r\n";
	FILE *pFile = fopen("C:\\Users\\VR\\Desktop\\platform-tools_r28.0.3-windows\\apk\\base\\assets\\bin\\Data\\sharedassets17.assets.split0", "rb");
	cout << "Parsing...\r\n";
	AssetsFile assetsFile(AssetsReaderFromFile, (LPARAM)pFile); // I tried &AssetsReaderFromFile too since I'm not sure what's right here but it makes no difference
	cout << "Getting file table...\r\n";
	AssetsFileTable assetsFileTable(&assetsFile);
	cout << "Getting reader...";
	AssetsFileReader reader = assetsFileTable.getReader();
	cout << "here5";
	AssetFileInfoEx* assetsFileInfo;// = assetsFileTable.getAssetInfo(240); // I know the ID - no need to search
	//cout << "file type: " << (*assetsFileInfo).curFileType << "     unity class: " << (*assetsFileInfo).inheritedUnityClass << "\r\n";



	std::vector<FILE*> filesToClose;
	std::vector<AssetsReplacer*> replacors;
	FILE *pReplaceFile;
	AssetsReplacer* assetReplacer;

	WIN32_FIND_DATAA data;
	HANDLE hFind;
	int filectr = 0;
	if ((hFind = FindFirstFileA(pattern.c_str(), &data)) != INVALID_HANDLE_VALUE) {
		do {
			std::string fname = data.cFileName;
			std::string fpath = string(path);
			std::string split = string(data.cFileName);
			fpath.append("\\");
			fpath.append(fname);

			std::string token = split.substr(0, split.find("_"));
			split.erase(0, split.find("_") + 1);
			std::string token2 = split.substr(0, split.find("_"));
			QWORD pathid = std::stoll(token);
			QWORD copypathid = std::stoll(token2);
			QWORD fsize = (QWORD)filesize(fpath.c_str());
			cout << token << ": " << fname << "\r\n";
			cout << "pathid: " << pathid << "\r\n";
			cout << "cloned pathid: " << copypathid << "\r\n";
			pReplaceFile = fopen(fpath.c_str(), "rb");
			cout << "fhandle: " << pReplaceFile << "\r\n";
			cout << "fsize: " << fsize << "\r\n";
			filesToClose.push_back(pReplaceFile);

			assetsFileInfo = assetsFileTable.getAssetInfo(copypathid);
			cout << "file type: " << (*assetsFileInfo).curFileType << "     unity class: " << (*assetsFileInfo).inheritedUnityClass << "\r\n";


			int h = 0xFFFFFFFC;
			cout << "here:" << h << "\r\n";
			assetReplacer = MakeAssetModifierFromFile(0, pathid, 0xFFFFFFFC, (*assetsFileInfo).inheritedUnityClass,
				pReplaceFile, 0, fsize);
			replacors.push_back(assetReplacer);
			filectr++;

		} while (FindNextFileA(hFind, &data) != 0);
		FindClose(hFind);
	}



	/*
	FILE *pReplaceFile = fopen("C:\\Users\\VR\\Desktop\\platform-tools_r28.0.3-windows\\aaaa\\Custom2LevelCollection.asset", "rb");
	filesToClose.push_back(pReplaceFile);

	AssetsReplacer* assetReplacer = MakeAssetModifierFromFile(0, 481008, (*assetsFileInfo).curFileType, (*assetsFileInfo).inheritedUnityClass,
		pReplaceFile, 0, (QWORD)filesize("C:\\Users\\VR\\Desktop\\platform-tools_r28.0.3-windows\\aaaa\\Custom2LevelCollection.asset")); // I expect that the size parameter refers to the file size but I couldn't check this until now
	replacors.push_back(assetReplacer);
	assetReplacer = MakeAssetModifierFromFile(0, 481009, (*assetsFileInfo).curFileType, (*assetsFileInfo).inheritedUnityClass,
		pReplaceFile, 0, (QWORD)filesize("C:\\Users\\VR\\Desktop\\platform-tools_r28.0.3-windows\\aaaa\\Custom2LevelCollection.asset")); // I expect that the size parameter refers to the file size but I couldn't check this until now
	cout << "here6\r\n";
	replacors.push_back(assetReplacer);
	*/
	cout << "Writing files...\r\n";
	FILE *pOutputFile = fopen("C:\\Users\\VR\\Desktop\\platform-tools_r28.0.3-windows\\7638-7516\\assets\\sharedassets17.assets.split0", "wb"); // Output in a temp-file and replace the original one afterwards since the original file should be locked right now
	assetsFile.Write(AssetsWriterToFile, (LPARAM)pOutputFile, 0, &replacors.front(), replacors.size() - 1, -1);
	cout << "Done writing\r\n";

	for (int i = 0; i < filesToClose.size(); i++) {
		fclose(filesToClose[i]);
	}


	fclose(pFile);
	fclose(pOutputFile);


}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
