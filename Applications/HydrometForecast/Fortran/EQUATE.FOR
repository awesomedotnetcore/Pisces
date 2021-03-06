	PROGRAM EQUATE
C
C	LOCATE AND SAVE FORECAST EQUATIONS TO BE RUN
C	SET UP FILE CONTAINING SITE NAMES FOR EACH TYPE OF DATA
!        1. CODE 17-UNREGULATED STREAM FLOW PUT IN, DATA IS HANDLED AS
!           A GAGE.  10/26/84
!	 2. ALLOW RECORDED STREAM FLOW (CODE 6) TO BE USED IN PLACE OF
!	    RESERVOIR OUTFLOW (CODE 3).  1/10/85
!        3. LOGIC CHANGE TO PUT AVERAGE ANTECEDENT RUNOFF IN AS CODE 17
!           (UNREGULATED FLOW) WHEN AVERAGE IS REQUIRED.  3/13/85
!	 4. 11/86
!	    CHANGE STATION NAME TO 12 CHARACTER CBTT NAME AND NUMERIC
!	    CODE TO 9 CHARACTER PCODE.
!	 5. PUT IN LOGIC TO TEST FOR BEGINNING YEAR FORECASTS START
!	    FOR A PARTICULAR SITE.  USED WHEN RUNNING HISTORICAL
!	    PERIOD OF TIME.   10/21/88 RAM
!
	DIMENSION IDFORE(3), ITYPE(4), IEQUAT(3), IDATA(4,8), WGHTM(12,8)
!       FOLLOWING JTYPS SUBSCRIPT FROM 8 TO 9.  #3 ABOVE
	DIMENSION NAMEST(3), ITYPS(8), SWGHT(8), JTYPS(9)
        DIMENSION KNAME(8),KTYPE(8)
	CHARACTER ITYPE*10, MNTHFC*12, IDFORE*10, NAMEST*10
	CHARACTER IEQUAT*10,KNAME*12,KTYPE*9,M*9
C
C	IN= FILE CONTAINING EQUATION NAMES TO BE RUN, IFC=FORECAST EQ.FILE
C	INFC=FORECAST EQUATIONS TO BE RUN, IST=SITE FILE
C	IOER=ERROR FILE
	DATA IN,IFC, INFC, IST / 5, 7, 14, 17 /, IOER/16/
!       FOLLOWING STATEMENT-ADDED 1HA.  #3 ABOVE
	DATA JTYPS /1HC, 1HG, 1HP, 1HR, 1HS, 1HM, 1HD,1HI,1HA/
	OPEN(UNIT=14,STATUS='NEW',CARRIAGECONTROL='LIST')
	OPEN(UNIT=17,STATUS='NEW',CARRIAGECONTROL='LIST')
	REWIND IN
	REWIND IFC
	WRITE(IOER,5)
5	FORMAT('0', 'ERROR FILE FOR EQUATE PROGRAM EXECUTION ' /)
C
C	READ FORECAST EQUATION FILE
C	TEST IF CORRECT FILE
C
	READ(IFC,12) ITYPE, IYEAR
12	FORMAT(4A10, I4 )
	NA = 7
	CALL CORFIL(NA,ITYPE)
	IF(IERR .EQ. 0) GO TO 100
	WRITE(IOER,13) ITYPE
13	FORMAT(//1X,'FILE SPECIFIED AS FORECAST EQUATION FILE NOT CORRECT'
	1 /1H , 'FILE SPECIFIED = ', 4A10 )
	STOP 13
100	iyr_flag = 0   ! #5. above
C
C	READ MONTH AND DAY OF FORECAST FROM THIS RUNS INPUT FILE
C
   	READ(IN,14) MNTHFC
14	FORMAT(A12)
	IERR = 0
	JERR = 0
	NMF = 0
	CALL DATE(DATEC)
C
C	READ NAME OF FORECAST FROM THIS RUNS INPUT FILE
C	AND PERCENT ABOVE AND BELOW NORMAL CONDITIONS
C
105	READ(IN,10,END=120) IDFORE, PCTABV, PCTBLW, lyear ! #5. above
10	FORMAT(3A10, 2F6.2, 8x, i4)                       ! #5. above
	GO TO 130
C	END OF RUN
120	REWIND IFC
        close(unit=14)
	REWIND IST
        close(unit=17)
	STOP 1000
C
C	LOCATE CORRECT EQUATION
C
130	if(lyear .gt. iyear) go to 105   ! #5. above
	if(iyr_flag .eq. 0) then         ! #5.
		iyr_flag = 1             ! #5.
	        WRITE(IST,6) IYEAR       ! #5.
6       	FORMAT(I4)               ! #5.
	end if                           ! #5.
	READ(IFC,15,END=135) IEQUAT, NMXS, NMBASN, NMSITE,IANTMO
15	FORMAT(3A10, 4I2)
	GO TO 140
135	WRITE(IOER,20) IDFORE
20	FORMAT(//' FORECAST EQUATION FILE DOES NOT CONTAIN FORECAST SPECIF
	1IED' /1H , 'FORECAST EQUATION SPCIFIED = ', 3A10 )
	JERR = 1
	REWIND IFC
	READ(IFC,12) IDUM
	GO TO 105
140	DO 145 I = 1,3
	IF(IDFORE(I) .NE. IEQUAT(I)) GO TO 146
145	CONTINUE
	GO TO 148
146	ITEST = 1
	IF(NMXS + NMBASN .GT.4 ) ITEST = 2
	ITEST = ITEST + NMSITE*2 + NMXS
	DO 147 I = 1,ITEST
	READ(IFC,12) IDUM
147	CONTINUE
	GO TO 130
C
C	READ NON-SITE DATA FROM FORECAST EQUATION  FILE
C
148	NMF = NMF + 1
	NMXST = NMXS + NMBASN
	READ(IFC,25) ((IDATA(I,J), I = 1,4), J = 1,NMXST)
25	FORMAT( 4(A2, I4, 2A3) )
	DO 150 J = 1,NMXS
	READ(IFC,30) (WGHTM(N,J), N = 1,12)
30	FORMAT(12F5.3 )
150	CONTINUE
C
C	WRITE NON-SITE DATA TO CURRENT FORECAST EQUATION  FILE
C
	WRITE(INFC,35) IEQUAT,NMXS,NMBASN,NMSITE,MNTHFC, IYEAR,
	1PCTABV, PCTBLW,IANTMO, NMF
35	FORMAT(3A10, 3I2, A12, I4,2F6.2,2I2)
	WRITE(INFC,25) ((IDATA(I,J), I = 1,4), J = 1,NMXST)
	DO 160 J = 1,NMXS
	WRITE(INFC,30) (WGHTM(N,J), N = 1,12)
160	CONTINUE
C
C	READ SITE DATA FROM FORECAST EQUATION FILE AND WRITE TO CURRENT
C	FORECAST EQUATION FILE.
C
	DO 190 K = 1,NMSITE
	READ(IFC,40) (NAMEST(I), I = 1,3), (ITYPS(J), SWGHT(J), J = 1,
	18 )
40	FORMAT(3A10, 8(A1, F5.3))
	READ(IFC,41) (KNAME(I),KTYPE(I),I=1,6)
41	FORMAT(6(A12,A9))
	WRITE(INFC,42)(NAMEST(I), I = 1,3), (ITYPS(J), SWGHT(J),
	1J = 1,NMXST)
42      FORMAT(3A10,8(A1,F5.3))
C
C	DEVELOPE SITE FILE
c	logic to keep from getting more than one set of eom and
c	outflow at one site
	do 164 j = 1,nmxst
	if(ityps(j) .ne. 1hR .and. ityps(j) .ne. 1hD
	1.and. ityps(j) .ne. 1hI ) go to 164
	i = j + 1
	do 162 n = i,nmxst
	if(ityps(n) .ne. 1hR .and. ityps(n) .ne. 1hD
	1.and. ityps(n) .ne. 1hI) go to 162
	ityps(n) = 1h 
162	continue
	go to 168
164     continue
C
!       FOLLOWING STATEMENT-CHANGED 1,8 TO 1,9.  #3 ABOVE
168	do 180 l = 1,9
	LA = 0
	DO 170 J = 1,NMXST
	IF(JTYPS(L) .NE. ITYPS(J) ) GO TO 170
	LA = J
	ITYPS(J) = 1H 
170	CONTINUE
	IF(LA .EQ. 0 ) GO TO 180
        IF(L.EQ.1) THEN
C
C       CANAL DATA (CANAL FLOW)
          M='QC       '
        ELSE IF(L.EQ.2) THEN
C
C       GAGE DATA (RECORDED STREAM FLOW)
          M='QM       '
        ELSE IF(L.EQ.3) THEN
C
C       PRECIPITATION DATA
          M = 'PM       '
        ELSE IF(L.EQ.4 .OR. L.EQ.7 .OR. L.EQ.8) THEN
C
C       CONTENT DATA (END-OF-MONTH CONTENT)
          M = 'AF       '
        ELSE IF(L.EQ.5) THEN
C
C       SNOW WATER CONTENT DATA (1st OF MONTH SNOW COURSE SWC)
          M = 'SU       '
        ELSE IF(L.EQ.6) THEN
C
C       SOIL MOISTURE DATA
          M = 'SM       '
!       FOLLOWING 4 STATEMENTS-ADDED.  #3 ABOVE
C	ANTECEDENT RUNOFF DATA - FUTURE MONTHS
	ELSE IF(L .EQ. 9) THEN
          IF(KTYPE(1) .EQ. '  ') GO TO 180
C	UNREGULATED FLOW
	  M='QU       '
        END IF
        DO 173 J = 1,8
	IF(L .EQ.5) THEN
	  IF(M.EQ.KTYPE(J) .OR. KTYPE(J) .EQ. 'SE       ') GO TO 171
	END IF
!       LOGIC CHANGE TO HANDLE CODE 17.  ie: #1  10/26/84
        IF(M.NE.KTYPE(J)) THEN
          IF(M .NE. 'QM       ' .OR. 
	1KTYPE(J) .NE. 'QU       ') GO TO 173
        END IF
        IF(M.EQ.'AF       ') GO TO 172
171     JA = J
        GO TO 175
172     JA = J + 1
!	 NEXT STATEMENT CHANGED  1/10/85 #2
        IF(KTYPE(JA).EQ.'OM       '.OR.
	1KTYPE(JA).EQ.'QM       ') GO TO 175
        GO TO 174
173     CONTINUE
C
C       ERROR ON FORECAST EQUATION FILE
174     WRITE(IOER,44) IEQUAT,NAMEST
44      FORMAT('0FORECAST EQUATION ',3A10/' SITE ',3A10,
	1' DOES NOT HAVE A CODE OR HAS AN INCORRECT CODE')
        GO TO 180
175     WRITE(IST,45) L, (NAMEST(I), I = 1,3), (IEQUAT(I), I = 1,3),NMF
	WRITE(IST,47) (KNAME(I),KTYPE(I),I=J,JA)
45	FORMAT(I2, 6A10, I2)
47	FORMAT(6(A12,A9))
180	CONTINUE
190	CONTINUE
	GO TO 105
	END
	SUBROUTINE CORFIL(NA,ITYPE)
C
C	TEST IF CORRECT FILE TYPE HAS BEEN SPECIFIED
C
	DIMENSION ITYPE(4), NTYPE(4,7)
	CHARACTER ITYPE*10, NTYPE*10
	DATA NTYPE /'PRECIPITAT',
	1'ION       ', 2*'          ', 'SNOW AND S', 'OIL MOISTU',
	2'RE        ', '          ', 'RUNOFF    ', 3*'          ',
	3'ESTIMATED ', 'PRECIPITAT', 'ION       ', '          ',
	4'ESTIMATED ', 'SNOW AND S', 'OIL MOISTU', 'RE        ',
	5'COEFFICIEN', 'T         ', 2*'          ',
	6'FORECAST E', 'QUATION   ', 2*'          '/
	IERR = 0
	DO 100 I = 1,4
	IF(ITYPE(I) .NE. NTYPE(I,NA)) GO TO 120
100	CONTINUE
	GO TO 140
120	IERR = 1
	JERR = 1
140	RETURN
	END
