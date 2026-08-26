// 會員系統(Identity + JWT)還沒做,先用共用 state 模擬登入/登出讓總覽頁兩種畫面都能預覽;
// 之後接真的登入狀態時,把這個檔案換成讀 JWT/session 的版本即可,呼叫端不用改
export const useDemoLoggedIn = () => useState<boolean>("demoLoggedIn", () => false)
