

###Döküman:

MyAnimeList API Rehberi ve Veri Çekme Stratejileri

Bu doküman, MyAnimeList (MAL) verileriyle çalışırken kullanılan yöntemleri, resmi ve gayri resmi API'leri, karşılaşılan yaygın sorunları (CORS, Rate Limit) ve projemizde kullandığımız "Akıllı Proxy Yönlendiricisi" mimarisini detaylı bir şekilde açıklar.

1. MyAnimeList Verisine Erişim Yöntemleri

MyAnimeList verilerine erişmek için temelde üç farklı yol vardır. Her birinin avantajları ve dezavantajları bulunur.

A. Resmi API (Official MAL API v2)

MyAnimeList'in sunduğu, geliştiricilerin kayıt olup Client ID alarak kullandığı resmi servistir.

Dokümantasyon: https://myanimelist.net/apiconfig/references/api/v2

Nasıl Çalışır? OAuth 2.0 protokolünü kullanır. Kullanıcıyı MAL sitesine yönlendirip "Uygulamaya İzin Ver" dedirtmeniz gerekir.

Dezavantajı: Sadece tarayıcıda çalışan (Backend sunucusu olmayan) basit HTML dosyalarında kullanmak çok zordur. Çünkü tarayıcı güvenliği (CORS) nedeniyle, doğrudan JavaScript ile istek atıldığında MAL sunucusu bağlantıyı reddeder.

Kullanım Alanı: Mobil uygulamalar, sunucu tabanlı (Node.js, Python) web siteleri.

B. Jikan API (Unofficial)

Topluluk tarafından geliştirilen, MAL verilerini kazıyıp (scrape) bize temiz JSON olarak sunan bir "köprü"dür.

Dokümantasyon: https://docs.api.jikan.moe/

Nasıl Çalışır? Siz Jikan'a istek atarsınız, Jikan sizin yerinize MAL'a gider, veriyi alır, düzenler ve size verir. CORS sorunu yoktur.

Dezavantajı: "Caching" (Önbellekleme) yapar. Yani bir kullanıcı listesini güncellediğinde, Jikan'ın bunu fark etmesi 24 saati bulabilir. Ayrıca çok sık istek atarsanız sizi geçici olarak engeller (Rate Limit).

Önemli Endpointler:

GET /users/{username}/animelist: Kullanıcının listesini çeker.

GET /anime/{id}: Bir animenin detaylarını verir.

GET /random/anime: Rastgele anime getirir.

C. load.json Yöntemi (Gizli Hazine)

MyAnimeList'in kendi web sitesinin, sayfaları yüklerken arka planda kullandığı, dokümantasyonu olmayan saf veri dosyasıdır.

URL: https://myanimelist.net/animelist/{username}/load.json?status=2&offset=0

Avantajı: Anlıktır. Jikan gibi bekleme süresi yoktur. Listeyi değiştirdiğiniz an buraya yansır.

Dezavantajı: Yine CORS engeline takılır. Bu yüzden bir "Proxy" (Aracı) olmadan tarayıcıdan çekilemez.

2. Projede Kullandığımız "Akıllı Proxy Yönlendiricisi" (3 Aşamalı Yöntem)

Senin projende yaşadığımız "Failed to fetch" ve "Liste boş" sorunlarını aşmak için geliştirdiğimiz sistem şu şekilde çalışır:

Mantık Şeması

Sistem tek bir kapıya güvenmez. Bir kapı kapalıysa, saniyesinde diğer kapıyı dener.

1. Aşama: Jikan API (En Temiz Yol)

İlk önce Jikan'a sorarız: "Bu kullanıcının listesini ver."

Eğer Jikan çalışıyorsa ve veri güncelse, veriyi alır ve ekrana basarız.

Sorun: Jikan bazen "Ben bu kullanıcıyı daha tanımıyorum, veritabanıma yazmam lazım" der ve boş liste döner. Bu durumda 2. aşamaya geçilir.

2. Aşama: CorsProxy.io (Hız Canavarı)

Jikan çalışmazsa, MyAnimeList'in load.json dosyasını CorsProxy.io servisi üzerinden çekeriz.

Bu servis, bizim yerimize MAL'a gidip o dosyayı alır ve tarayıcının kabul edeceği şekilde bize sunar.

Genelde çok hızlıdır ve Jikan'ın bekleme süresine takılmaz.

3. Aşama: AllOrigins (Son Çare)

Eğer CorsProxy.io da yoğunluktan dolayı yanıt vermezse, AllOrigins servisini devreye sokarız.

Bu servis veriyi biraz farklı bir formatta (JSON string) döndürür, bu yüzden kodda bunu özel olarak işleyip (parse edip) kullanırız.

Kod İçindeki Karşılığı

Projedeki PROXY_SERVICES dizisi bu "yedek anahtarları" tutar:

const PROXY_SERVICES = [
    { 
        name: 'CorsProxy.io', 
        getUrl: (target) => `https://corsproxy.io/?${encodeURIComponent(target)}` 
    },
    { 
        name: 'AllOrigins', 
        getUrl: (target) => `https://api.allorigins.win/raw?url=${encodeURIComponent(target)}` 
    }
];


Kod bir döngü (loop) içinde sırasıyla bu servisleri dener. Biri hata verirse catch bloğuna düşer, hatayı yutar ve döngü bir sonrakine geçer. Hepsi hata verirse ancak o zaman "Başarısız" uyarısı verir.

3. MyAnimeList Veri Yapısı ve Endpoint Analizi

Projede kullandığımız verilerin ne anlama geldiğini inceleyelim.

load.json Parametreleri

https://myanimelist.net/animelist/Mamito_Aga/load.json?offset=0&status=2

username: Kimin listesi çekilecek? (Örn: Mamito_Aga)

status: Hangi kategori?

1: Watching (İzleniyor)

2: Completed (Tamamlandı) - Bizim kullandığımız

3: On Hold (Beklemede)

4: Dropped (Bırakıldı)

6: Plan to Watch (İzlenecek)

offset: Sayfalama mantığı. MAL bu dosyada her seferinde en fazla 300 anime verir.

İlk istek: offset=0 (1-300 arası)

İkinci istek: offset=300 (301-600 arası)

Üçüncü istek: offset=600 (601-900 arası)

Gelen JSON Verisi (Örnek)

Veri bize geldiğinde şu formatta olur, biz bunu Jikan formatına dönüştürürüz (Mapping):

[
  {
    "anime_id": 5114,
    "anime_title": "Fullmetal Alchemist: Brotherhood",
    "anime_image_path": "[https://cdn.myanimelist.net/images/anime/1223/96541.jpg](https://cdn.myanimelist.net/images/anime/1223/96541.jpg)",
    "score": 10,
    "status": 2,
    "is_rewatching": 0,
    "num_watched_episodes": 64
  }
]


Dönüştürme (Mapping) İşlemi:
Kodumuzda mappedItems kısmında bu veriyi alıp kendi uygulamamızın anlayacağı dile çeviriyoruz:

anime_id -> mal_id

anime_title -> title

anime_image_path -> images.jpg.image_url

4. Özet ve Tavsiyeler

Bu proje, Front-end geliştirmede sıkça karşılaşılan API sorunlarına karşı "Dayanıklı Kod Yazma" (Robust Coding) örneğidir.

Asla Tek Kaynağa Güvenme: Harici API'ler her zaman çökebilir. Yedek (Fallback) mekanizmaları hayat kurtarır.

Kullanıcıyı Bilgilendir: Veri yüklenirken "Yükleniyor", hata aldığında "Şunu deniyorum..." gibi mesajlar vermek, kullanıcının "Bozuldu bu" diyip çıkmasını engeller.

Proxy Kullanımı: Sunucusuz (Serverless) veya sadece HTML/JS projelerinde CORS hatasını aşmanın en pratik yolu güvenilir Proxy servisleridir.

Artık elinde çalışan, hataya dayanıklı ve sürükle-bırak özellikli modern bir Tier List uygulaman var!




###Kod Kullanım örenği:



<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>MyAnimeList Tier Ranker - Ultimate V2</title>
    <script src="https://cdn.tailwindcss.com"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/Sortable/1.15.0/Sortable.min.js"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;600;800&display=swap');
        body { font-family: 'Inter', sans-serif; background-color: #121212; color: #e0e0e0; }
        
        /* Tier Renkleri */
        .tier-label-10 { background-color: #ff7f7f; color: #000; }
        .tier-label-9  { background-color: #ffbf7f; color: #000; }
        .tier-label-8  { background-color: #ffdf7f; color: #000; }
        .tier-label-7  { background-color: #ffff7f; color: #000; }
        .tier-label-6  { background-color: #bfff7f; color: #000; }
        .tier-label-5  { background-color: #7fff7f; color: #000; }
        .tier-label-4  { background-color: #7fffff; color: #000; }
        .tier-label-3  { background-color: #7fbfff; color: #000; }
        .tier-label-2  { background-color: #7f7fff; color: #fff; }
        .tier-label-1  { background-color: #bf7fff; color: #fff; }

        .tier-content { counter-reset: anime-rank; }
        
        .anime-card { position: relative; transition: transform 0.2s, box-shadow 0.2s; }
        .anime-card:hover { transform: translateY(-2px); box-shadow: 0 4px 12px rgba(0,0,0,0.5); z-index: 10; }
        
        /* Sol üstteki sıralama numarası */
        .anime-card::before {
            counter-increment: anime-rank;
            content: counter(anime-rank);
            position: absolute; top: 0; left: 0;
            background-color: rgba(220, 38, 38, 0.9); color: white;
            font-size: 0.75rem; font-weight: bold;
            padding: 2px 6px; border-bottom-right-radius: 4px;
            z-index: 20; pointer-events: none;
        }

        .sortable-ghost { opacity: 0.4; background-color: #333; }
        .sortable-drag { cursor: grabbing; }
        ::-webkit-scrollbar { width: 8px; height: 8px; }
        ::-webkit-scrollbar-track { background: #1a1a1a; }
        ::-webkit-scrollbar-thumb { background: #444; border-radius: 4px; }
        ::-webkit-scrollbar-thumb:hover { background: #555; }
    </style>
</head>
<body class="min-h-screen p-4 md:p-8">

    <div class="max-w-7xl mx-auto mb-8 text-center">
        <h1 class="text-4xl font-extrabold mb-2 text-white tracking-tight">
            <span class="text-blue-500">MyAnimeList</span> Tier Ranker
        </h1>
        <p class="text-gray-400 mb-6 text-sm">Akıllı Proxy Yönlendiricisi (3 Farklı Sunucu)</p>

        <div class="flex flex-col md:flex-row justify-center items-center gap-4 bg-[#1e1e1e] p-4 rounded-xl shadow-lg inline-block mx-auto border border-gray-800">
            <div class="relative">
                <i class="fa-solid fa-user absolute left-3 top-3 text-gray-400"></i>
                <input type="text" id="username" value="Mamito_Aga" 
                    class="pl-10 pr-4 py-2 bg-[#2d2d2d] border border-gray-700 rounded-lg focus:outline-none focus:border-blue-500 text-white w-64 placeholder-gray-500"
                    placeholder="MAL Kullanıcı Adı">
            </div>
            <button onclick="startDataFetch()" 
                class="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-6 rounded-lg transition-all flex items-center gap-2 shadow-lg active:scale-95">
                <i class="fa-solid fa-download"></i> Verileri Getir
            </button>
        </div>
        
        <div id="status-msg" class="mt-4 min-h-[24px] text-sm font-medium text-yellow-400 flex flex-col items-center justify-center gap-2"></div>
    </div>

    <div id="tier-container" class="max-w-7xl mx-auto space-y-3 select-none"></div>

    <script>
        document.addEventListener('DOMContentLoaded', () => createTierRows());

        function createTierRows() {
            const container = document.getElementById('tier-container');
            container.innerHTML = '';
            for (let i = 10; i >= 1; i--) {
                const row = document.createElement('div');
                row.className = 'flex bg-[#000] border border-[#333] min-h-[130px] group overflow-hidden rounded-md';
                
                const label = document.createElement('div');
                label.className = `tier-label-${i} w-20 md:w-24 flex-shrink-0 flex items-center justify-center text-2xl md:text-3xl font-black shadow-inner border-r border-black/20`;
                label.innerText = i;

                const content = document.createElement('div');
                content.className = 'tier-content flex-1 p-2 flex flex-wrap gap-2 bg-[#1a1a1a] content-start items-start transition-colors';
                content.id = `tier-${i}`;
                content.setAttribute('data-score', i);

                row.appendChild(label);
                row.appendChild(content);
                container.appendChild(row);

                new Sortable(content, {
                    group: 'anime', animation: 150, ghostClass: 'sortable-ghost', dragClass: 'sortable-drag',
                    delay: 0, forceFallback: false, fallbackOnBody: true, swapThreshold: 0.65
                });
            }
        }

        const wait = (ms) => new Promise(resolve => setTimeout(resolve, ms));

        // --- YENİ PROXY SİSTEMİ ---
        // Farklı proxy servisleri tanımlandı. Biri çalışmazsa diğeri denenecek.
        const PROXY_SERVICES = [
            { 
                name: 'AllOrigins (Raw)', 
                getUrl: (target) => `https://api.allorigins.win/raw?url=${encodeURIComponent(target)}`,
                parse: async (res) => await res.json() // Raw direkt JSON döner
            },
            { 
                name: 'CorsProxy.io', 
                getUrl: (target) => `https://corsproxy.io/?${encodeURIComponent(target)}`,
                parse: async (res) => await res.json()
            },
            { 
                name: 'CodeTabs', 
                getUrl: (target) => `https://api.codetabs.com/v1/proxy?quest=${encodeURIComponent(target)}`,
                parse: async (res) => await res.json()
            }
        ];

        // Ana Fonksiyon
        async function startDataFetch() {
            const username = document.getElementById('username').value.trim();
            const statusMsg = document.getElementById('status-msg');
            const btn = document.querySelector('button');

            if (!username) { statusMsg.innerText = "Kullanıcı adı giriniz."; return; }

            createTierRows();
            btn.disabled = true; btn.classList.add('opacity-50', 'cursor-not-allowed');
            statusMsg.innerHTML = '<div class="flex items-center gap-2"><i class="fa-solid fa-spinner fa-spin"></i> Jikan API ile deneniyor...</div>';
            statusMsg.className = "mt-4 text-sm font-medium text-blue-400 flex flex-col items-center";

            try {
                // 1. Önce Jikan'ı dene (En temiz veri kaynağı)
                await fetchViaJikan(username, statusMsg);
            } catch (jikanError) {
                console.warn("Jikan başarısız oldu, Proxy Router devreye giriyor...", jikanError);
                
                // 2. Jikan başarısızsa Proxy Router'ı çalıştır
                try {
                    await fetchViaProxyRouter(username, statusMsg);
                } catch (routerError) {
                    console.error(routerError);
                    statusMsg.innerHTML = `
                        <div class="text-red-400 mb-2 text-center flex flex-col gap-1">
                            <div><i class="fa-solid fa-circle-exclamation"></i> Tüm sunucular denendi ve başarısız oldu.</div>
                            <div class="text-xs text-gray-400">Olası Nedenler: <br>1. AdBlocker veya VPN engelliyor olabilir.<br>2. MyAnimeList geçici olarak yanıt vermiyor.<br>3. Kullanıcı listeniz 'Gizli' (Private).</div>
                        </div>
                        <button onclick="loadSampleData()" class="bg-gray-700 hover:bg-gray-600 text-white text-xs py-1 px-3 rounded transition-colors border border-gray-600 mt-2">
                            <i class="fa-solid fa-flask"></i> Örnek Veri Yükle
                        </button>
                    `;
                }
            } finally {
                btn.disabled = false; btn.classList.remove('opacity-50', 'cursor-not-allowed');
            }
        }

        // Yöntem 1: Jikan API
        async function fetchViaJikan(username, statusMsg) {
            let allAnime = [];
            let page = 1;
            let hasNextPage = true;
            let emptyRetryCount = 0;

            while (hasNextPage) {
                const url = `https://api.jikan.moe/v4/users/${username}/animelist?status=completed&page=${page}&limit=25&t=${Date.now()}`;
                let response = await fetch(url);

                if (response.status === 429) {
                    await wait(2000); response = await fetch(url); // Retry
                    if (response.status === 429) throw new Error("Jikan Rate Limit");
                }

                if (!response.ok) throw new Error(`Jikan API Error: ${response.status}`);

                const data = await response.json();
                const items = data.data || [];

                // Liste boşsa hemen hataya düş (vakit kaybetme, proxy dene)
                if (items.length === 0 && page === 1) {
                     if (emptyRetryCount < 1) { 
                        emptyRetryCount++;
                        await wait(1000); continue;
                    }
                    throw new Error("Jikan Empty List");
                }

                if (items.length === 0) break;

                allAnime = [...allAnime, ...items];
                distributeAnime(allAnime);
                
                statusMsg.innerHTML = `<div class="flex items-center gap-2"><i class="fa-solid fa-cloud-arrow-down"></i> Jikan: ${allAnime.length} anime yüklendi...</div>`;
                
                hasNextPage = data.pagination && data.pagination.has_next_page;
                page++;
                if (hasNextPage) await wait(400);
            }
            statusMsg.innerHTML = `<div class="flex items-center gap-2 text-green-400"><i class="fa-solid fa-check"></i> Tamamlandı! ${allAnime.length} anime (Jikan).</div>`;
        }

        // Yöntem 2: Proxy Router (Sırayla tüm proxy'leri dener)
        async function fetchViaProxyRouter(username, statusMsg) {
            let lastError = null;
            
            // Proxy listesi üzerinde dön
            for (const proxy of PROXY_SERVICES) {
                try {
                    statusMsg.innerHTML = `<div class="flex items-center gap-2 text-orange-400"><i class="fa-solid fa-network-wired"></i> Sunucu deneniyor: ${proxy.name}...</div>`;
                    
                    // Bu proxy ile veriyi çekmeyi dene
                    await fetchMalDataViaSpecificProxy(username, statusMsg, proxy);
                    
                    // Eğer hata almadan buraya geldiyse, işlem başarılı demektir. Döngüyü kır.
                    return; 
                } catch (error) {
                    console.warn(`${proxy.name} başarısız oldu:`, error);
                    lastError = error;
                    // Hata oldu, döngü devam eder ve bir sonraki proxy'ye geçer
                }
            }
            
            // Eğer döngü bitti ve hiçbiri çalışmadıysa
            throw lastError || new Error("Tüm proxy servisleri başarısız.");
        }

        // Belirli bir proxy üzerinden veriyi çeken alt fonksiyon
        async function fetchMalDataViaSpecificProxy(username, statusMsg, proxyService) {
            let allAnime = [];
            let offset = 0;
            let hasMore = true;
            
            while(hasMore) {
                const malUrl = `https://myanimelist.net/animelist/${username}/load.json?offset=${offset}&status=2`;
                const finalUrl = proxyService.getUrl(malUrl);

                const response = await fetch(finalUrl);
                if (!response.ok) throw new Error(`HTTP Error: ${response.status}`);

                // Proxy'den gelen veriyi servise özel yöntemle parse et
                let items;
                try {
                    items = await proxyService.parse(response);
                } catch (e) {
                    // Bazı proxy'ler JSON yerine string dönerse
                    const text = await response.text();
                    try { items = JSON.parse(text); } catch(e2) { throw new Error("JSON Parse Error"); }
                }

                // AllOrigins bazen wrapper içinde döner
                if (items.contents) {
                    try { items = JSON.parse(items.contents); } catch(e) {}
                }

                if (!items || !Array.isArray(items) || items.length === 0) {
                    hasMore = false;
                    break;
                }

                // Mapping: MAL formatını Jikan formatına çevir
                const mappedItems = items.map(item => ({
                    score: item.score, 
                    anime: {
                        mal_id: item.anime_id,
                        title: item.anime_title,
                        url: `https://myanimelist.net${item.anime_url}`,
                        images: {
                            jpg: { image_url: item.anime_image_path }
                        }
                    }
                }));

                allAnime = [...allAnime, ...mappedItems];
                distributeAnime(allAnime);

                // Ekrana bilgi bas (Hangi proxy ile çalıştığını göster)
                statusMsg.innerHTML = `<div class="flex items-center gap-2 text-orange-300"><i class="fa-solid fa-satellite-dish fa-spin"></i> ${proxyService.name} üzerinden çekiliyor (${allAnime.length})...</div>`;

                if (items.length < 300) {
                    hasMore = false;
                } else {
                    offset += 300;
                    await wait(1500); // Proxy'yi kızdırmamak için biraz daha uzun bekle
                }
            }

            if (allAnime.length === 0) throw new Error("Liste boş.");
            statusMsg.innerHTML = `<div class="flex items-center gap-2 text-green-400"><i class="fa-solid fa-check"></i> Tamamlandı! ${allAnime.length} anime (${proxyService.name}).</div>`;
        }

        function distributeAnime(list) {
            // DOM temizle
            for (let i = 1; i <= 10; i++) document.getElementById(`tier-${i}`).innerHTML = '';

            // Veriyi Grupla
            const groups = {};
            for (let i = 1; i <= 10; i++) groups[i] = [];

            list.forEach(item => {
                const score = item.score;
                if (score > 0 && score <= 10) {
                    groups[score].push({
                        id: item.anime.mal_id,
                        title: item.anime.title,
                        image: item.anime.images.jpg.image_url,
                        url: item.anime.url
                    });
                }
            });

            // DOM Bas
            for (let i = 10; i >= 1; i--) {
                const container = document.getElementById(`tier-${i}`);
                if(groups[i]) {
                    groups[i].sort((a, b) => a.title.localeCompare(b.title));
                    groups[i].forEach(anime => {
                        const card = document.createElement('div');
                        card.className = 'anime-card w-20 cursor-grab active:cursor-grabbing flex flex-col bg-[#252525] rounded overflow-hidden group relative';
                        card.innerHTML = `
                            <div class="w-full h-28 overflow-hidden">
                                <img src="${anime.image}" alt="${anime.title}" class="w-full h-full object-cover pointer-events-none select-none" onerror="this.src='https://placehold.co/100x140?text=No+Img'">
                            </div>
                            <div class="p-1 text-[10px] text-center leading-tight text-gray-300 h-8 flex items-center justify-center overflow-hidden">
                                <span class="line-clamp-2">${anime.title}</span>
                            </div>
                            <a href="${anime.url}" target="_blank" class="absolute top-1 right-1 bg-black/70 text-white text-[8px] p-1 rounded opacity-0 group-hover:opacity-100 transition-opacity z-30 hover:bg-blue-600">
                                <i class="fa-solid fa-arrow-up-right-from-square"></i>
                            </a>
                        `;
                        container.appendChild(card);
                    });
                }
            }
        }

        function loadSampleData() {
            const statusMsg = document.getElementById('status-msg');
            createTierRows();
            const sampleData = [
                { score: 10, anime: { mal_id: 1, title: "Fullmetal Alchemist: Brotherhood", url:"#", images:{jpg:{image_url:"https://cdn.myanimelist.net/images/anime/1208/94745.jpg"}} } },
                { score: 9, anime: { mal_id: 3, title: "Hunter x Hunter (2011)", url:"#", images:{jpg:{image_url:"https://cdn.myanimelist.net/images/anime/1337/99013.jpg"}} } }
            ];
            distributeAnime(sampleData);
            statusMsg.innerHTML = '<div class="text-green-400">Örnek veri yüklendi.</div>';
        }
    </script>
</body>
</html>