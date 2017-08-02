import { Component, OnInit } from '@angular/core';

// Constants which will be fetched from Functions endpoints
let STAGE;
let SITEURL;
let STORAGE_URL ;
let CONTAINER_SAS;
let INPUTCONTAINERNAME;
let OUTPUTCONTAINERNAME;

@Component({
    selector: 'home',
    templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {

    // when HomeComponent is ready
    ngOnInit() {
        // console.log("Dropzone configured");
        // var myDropzone = new Dropzone("form#dropzone", {
        //     autoQueue: false,
        //     url: (files) => {
        //         // This shouldn't be used since we cancel all uploads 
        //         // via Dropzone and hijack them for our own upload
        //         // since Dropzone uses multi-part uploads :(
        //         return `${STORAGE_URL}${INPUTCONTAINERNAME}/${files[0].name}${CONTAINER_SAS}`;
        //     },
        //     headers: {
        //         "x-ms-blob-type": "BlockBlob",
        //         "Content-Type": "image/jpeg"
        //     },
        //     accept: (file, done) => {
        //         if (file.name.endsWith(".jpg")) {
        //             done();
        //         } else {
        //             alert("We only accept files which end with .jpg");
        //             done("We only accept files which end with .jpg");
        //         }
        //     },
        //     clickable: true,
        //     method: "PUT",
        //     init: function () {
        //         this.on("success", function (file, response) {
        //             console.log(file.name);
        //         });
        //         this.on("addedfile", function (file) {
        //             const that = this;
        //             if (!file.name.endsWith(".jpg")) {
        //                 this.removeFile(file);
        //                 return;
        //             }
        //             const name = uploadFile(file, (err) => {
        //                 console.log("File uploaded");
        //                 that.removeFile(file);
        //                 if (err) {
        //                     throw err;
        //                 }
        //                 enqueueCard(name, file.name);
        //             });
        //         });
        //     }
        // });

    }

    // enqueueCard(name, fileName) {
    //     // 1. Show loading element
    //     const images = document.getElementById("images");
    //     const imageDiv = document.createElement("div");
    //     imageDiv.classList.add("card");
    //     const loading = document.createElement("span");
    //     loading.classList.add("loading");
    //     loading.classList.add("fa");
    //     loading.classList.add("fa-refresh");
    //     imageDiv.appendChild(loading);
    //     images.appendChild(imageDiv);
    //     const rand = Math.floor(Math.random() * 1000);
    //     imageDiv.id = `image-${name}-${rand}`;
    
    //     // 3. Poll for image to show
    //     let interval = setInterval(() => {
    //         console.log("Looking for file");
    //         this.getImage(name, imageDiv.id, (done) => {
    //             if (done) {
    //                 console.log("Clearing interval");
    //                 clearInterval(interval);
    //             }
    //         });
    //     }, 3000);
    // }

    // uploadFile(file, cb) {
    //     const personName = prompt("What name should appear on the card?");
    //     const title = prompt("What's their title?");
    //     const name = this.generateUUID() + '.jpg'
    //     const xhr = new XMLHttpRequest();
    //     xhr.open("PUT", `${STORAGE_URL}${INPUTCONTAINERNAME}/${name}${CONTAINER_SAS}`);
    //     xhr.setRequestHeader('x-ms-blob-type', 'BlockBlob');
    //     xhr.setRequestHeader('Content-Type', 'image/jpeg');
    //     xhr.send(file);
    //     xhr.onload = (e) => {
    //         if (xhr.status > 300) {
    //             // Error
    //             const err = new Error(`Image upload returned ${xhr.status}`);
    //             return cb(err);
    //         }
    //         this.startImageProcessing(name, title, personName, cb);
    //     }
    //     return name;
    // }

    // startImageProcessing(file, title, name, cb) {
    //     const xhr = new XMLHttpRequest();
    //     xhr.open("POST", `${SITEURL}/api/RequestImageProcessing`);
    //     xhr.setRequestHeader('Content-Type', 'application/json');
    //     xhr.send(JSON.stringify({
    //         BlobName: file,
    //         Title: title,
    //         PersonName: name
    //     }));
    //     xhr.onload = (e) => {
    //         if (xhr.status > 300) {
    //             cb(new Error("Bad response from RequestImageProcessing"));
    //         }
    //         cb(null);
    //     }
    // }

    // getImage(name, imageId, cb) {
    //     const xhr = new XMLHttpRequest();
    //     const imageUrl = `${STORAGE_URL}${OUTPUTCONTAINERNAME}/${name}`;
    //     xhr.open("GET", imageUrl);
    //     xhr.responseType = "blob";
    //     xhr.onload = (e) => {
    //         if (xhr.status != 200) {
    //             console.log("Image does not exist yet");
    //             return cb(false);
    //         }
    //         let images = document.getElementById(imageId);
    //         images.innerHTML = '';
    
    //         let link = document.createElement('a');
    //         link.href = imageUrl;
    //         let image = document.createElement('img');
    //         image.src = imageUrl;
    //         image.setAttribute("height", "300px");
    
    //         link.appendChild(image);
    //         images.appendChild(link);
    //         console.log("Image created");
    //         return cb(true);
    //     }
    //     var results = xhr.send();
    // }

    // generateUUID() {
    //     let values = [];
    //     for (let i = 0; i < 16; i++) {
    //         values.push(Math.floor(Math.random() * 100) % 10);
    //     }
    //     return values.join("");
    // }

    // updateTitle() {
    //     const title = document.getElementById("page-title");
    //     if (STAGE && STAGE != "PROD") {
    //         title.innerHTML = `Coder Cards - ${STAGE}`;
    //     }
    // }

    // configureSettings(isLocal) {
    //     try {
    //         const xhr = new XMLHttpRequest();
    //         const URL = isLocal ? "http://localhost:7071/api/Settings" : "/api/Settings";
    //         xhr.open("GET", URL);
    //         xhr.setRequestHeader('Content-Type', 'application/json');
    //         xhr.onload = (e) => {
    //             if (xhr.status != 200) {
    //                 if (!isLocal) {
    //                     this.configureSettings(true);
    //                 } else {
    //                     throw new Error("Bad response from server for Settings");
    //                 }
    //                 return;
    //             }
    //             console.info(xhr.response);
    //             let response = xhr.response;
    //             if (typeof xhr.response === "string") {
    //                 response = JSON.parse(xhr.response);
    //             }
    //             STAGE = response.Stage;
    //             SITEURL = isLocal ? response.SiteURL : "";
    //             STORAGE_URL = response.StorageURL;
    //             CONTAINER_SAS = response.ContainerSAS;
    //             INPUTCONTAINERNAME = response.InputContainerName;
    //             OUTPUTCONTAINERNAME = response.OutputContainerName;
    //             this.updateTitle();
    //         }
    //         xhr.send();
    //     } catch (e) {
    //         console.log(e);
    //     }
    // }
}